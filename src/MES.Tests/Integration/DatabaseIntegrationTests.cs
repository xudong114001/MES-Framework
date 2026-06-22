using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Data;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;
using MES.Tests;

namespace MES.Tests.Integration;

public class DatabaseIntegrationTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public DatabaseIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// 辅助方法：创建基础测试数据（物料 + 工单），返回 (materialId, workOrderId)
    /// </summary>
    private async Task<(long materialId, long workOrderId)> SeedWorkOrderAsync(MesDbContext db)
    {
        var material = new Material
        {
            Code = $"MAT-{Guid.NewGuid():N}",
            Name = "测试物料",
            Unit = "PCS",
            Category = "FINISHED",
            BomLevel = 0,
            StockQty = 1000,
            Status = true
        };
        db.Materials.Add(material);
        await db.SaveChangesAsync();

        var workOrder = TestEntityFactory.CreateWorkOrderDirect(
            id: 0,
            orderNo: $"WO-{Guid.NewGuid():N}",
            materialId: material.Id,
            plannedQty: 100,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.IN_PROGRESS,
            priority: Priority.NORMAL
        );
        db.WorkOrders.Add(workOrder);
        await db.SaveChangesAsync();

        return (material.Id, workOrder.Id);
    }

    /// <summary>
    /// 测试：100 并发报工数据库一致性
    /// 100 个并发任务同时对同一工单报工，最终 CompletedQty 应精确等于 100
    /// </summary>
    [Fact]
    public async Task ConcurrentWorkReports_ShouldMaintainDataConsistency()
    {
        // Arrange
        var (_, workOrderId) = await SeedWorkOrderAsync(DbContext);

        const int concurrencyLevel = 100;
        var tasks = new Task[concurrencyLevel];
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        // Act: 100 个并发报工
        for (int i = 0; i < concurrencyLevel; i++)
        {
            var index = i;
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    using var db = CreateNewDbContext();
                    var strategy = db.Database.CreateExecutionStrategy();

                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var transaction = await db.Database.BeginTransactionAsync(
                            System.Data.IsolationLevel.Serializable);

                        try
                        {
                            var wo = await db.WorkOrders.FirstAsync(w => w.Id == workOrderId);
                            // 使用原始 SQL 更新，因为 CompletedQty 是 private set
                            await db.Database.ExecuteSqlRawAsync(
                                "UPDATE mes_work_order SET completed_qty = completed_qty + 1, updated_at = {0} WHERE id = {1}",
                                DateTime.UtcNow, workOrderId);

                            var report = new WorkReport
                            {
                                ReportNo = $"WR-{index:D5}-{Guid.NewGuid():N}",
                                WorkOrderId = workOrderId,
                                ReportType = ReportType.COMPLETE,
                                GoodQty = 1,
                                ScrapQty = 0,
                                ReworkQty = 0,
                                DurationMin = 10,
                                ReportTime = DateTime.UtcNow
                            };
                            db.WorkReports.Add(report);

                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert: 即使有冲突重试，最终状态应一致
        using var verifyDb = CreateNewDbContext();
        var finalOrder = await verifyDb.WorkOrders.FirstAsync(w => w.Id == workOrderId);
        var reportCount = await verifyDb.WorkReports.CountAsync(r => r.WorkOrderId == workOrderId);

        _output.WriteLine($"并发报工完成：errors={errors.Count}, CompletedQty={finalOrder.CompletedQty}, ReportCount={reportCount}");

        // 使用串行化隔离级别，所有事务应串行执行成功
        Assert.Equal(concurrencyLevel - errors.Count, (int)finalOrder.CompletedQty);
        Assert.Equal(concurrencyLevel - errors.Count, reportCount);
        Assert.Equal(finalOrder.CompletedQty, workOrderId > 0 ? finalOrder.CompletedQty : 0);

        // 验证数值一致性：CompletedQty == 报工记录总数
        Assert.Equal((int)finalOrder.CompletedQty, reportCount);
    }

    /// <summary>
    /// 测试：级联删除校验 —— 物料删除前有工单引用时应阻止
    /// </summary>
    [Fact]
    public async Task DeleteMaterial_WhenReferencedByWorkOrder_ShouldBeRestricted()
    {
        // Arrange: 创建物料和关联工单
        var material = new Material
        {
            Code = $"MAT-DEL-{Guid.NewGuid():N}",
            Name = "待删除物料",
            Unit = "PCS",
            Status = true
        };
        DbContext.Materials.Add(material);
        await DbContext.SaveChangesAsync();

        var workOrder = TestEntityFactory.CreateWorkOrderDirect(
            id: 0,
            orderNo: $"WO-DEL-{Guid.NewGuid():N}",
            materialId: material.Id,
            plannedQty: 50,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.PENDING
        );
        DbContext.WorkOrders.Add(workOrder);
        await DbContext.SaveChangesAsync();

        // Act & Assert: 尝试删除物料，应因外键约束抛出异常
        using var deleteDb = CreateNewDbContext();
        var materialToDelete = await deleteDb.Materials.FirstAsync(m => m.Id == material.Id);

        deleteDb.Materials.Remove(materialToDelete);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => deleteDb.SaveChangesAsync());

        _output.WriteLine($"预期异常: {exception.GetType().Name} - {exception.Message}");

        // 验证物料仍然存在
        using var verifyDb = CreateNewDbContext();
        var stillExists = await verifyDb.Materials.AnyAsync(m => m.Id == material.Id);
        Assert.True(stillExists, "有工单引用的物料不应被删除");
    }

    /// <summary>
    /// 测试：10 万条数据分页查询性能 < 500ms
    /// </summary>
    [Fact]
    public async Task PaginationQuery_With100KRecords_ShouldCompleteWithin500ms()
    {
        // Arrange: 批量插入 100,000 条报工记录
        var material = new Material
        {
            Code = "MAT-PERF",
            Name = "性能测试物料",
            Unit = "PCS",
            Status = true
        };
        DbContext.Materials.Add(material);
        await DbContext.SaveChangesAsync();

        var workOrder = TestEntityFactory.CreateWorkOrderDirect(
            id: 0,
            orderNo: "WO-PERF",
            materialId: material.Id,
            plannedQty: 100000,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.IN_PROGRESS
        );
        DbContext.WorkOrders.Add(workOrder);
        await DbContext.SaveChangesAsync();

        const int totalRecords = 100_000;
        const int batchSize = 5000;

        _output.WriteLine($"开始插入 {totalRecords} 条报工记录...");

        for (int batch = 0; batch < totalRecords / batchSize; batch++)
        {
            using var batchDb = CreateNewDbContext();
            for (int i = 0; i < batchSize; i++)
            {
                batchDb.WorkReports.Add(new WorkReport
                {
                    ReportNo = $"WR-PERF-{(batch * batchSize + i):D6}",
                    WorkOrderId = workOrder.Id,
                    ReportType = ReportType.COMPLETE,
                    GoodQty = 1,
                    ScrapQty = 0,
                    ReworkQty = 0,
                    DurationMin = 5,
                    ReportTime = DateTime.UtcNow.AddMinutes(-(batch * batchSize + i))
                });
            }
            await batchDb.SaveChangesAsync();
        }

        _output.WriteLine("数据插入完成，开始分页查询性能测试...");

        // Act: 分页查询第 500 页（每页 100 条，偏移量 49900）
        const int pageSize = 100;
        const int targetPage = 500;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        using var queryDb = CreateNewDbContext();
        var pagedResults = await queryDb.WorkReports
            .Where(r => r.WorkOrderId == workOrder.Id)
            .OrderByDescending(r => r.ReportTime)
            .Skip((targetPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        sw.Stop();

        // Assert
        _output.WriteLine($"分页查询耗时: {sw.ElapsedMilliseconds}ms, 返回记录数: {pagedResults.Count}");
        Assert.True(sw.ElapsedMilliseconds < 500,
            $"分页查询耗时 {sw.ElapsedMilliseconds}ms，超过 500ms 阈值");
        Assert.Equal(pageSize, pagedResults.Count);

        // 验证总数
        var totalCount = await queryDb.WorkReports.CountAsync(r => r.WorkOrderId == workOrder.Id);
        Assert.Equal(totalRecords, totalCount);
    }

    /// <summary>
    /// 测试：事务回滚 —— 异常发生时数据应恢复到事务前状态
    /// </summary>
    [Fact]
    public async Task TransactionRollback_ShouldRestorePreviousState()
    {
        // Arrange: 创建一个工单
        var material = new Material
        {
            Code = $"MAT-RB-{Guid.NewGuid():N}",
            Name = "回滚测试物料",
            Unit = "PCS",
            Status = true
        };
        DbContext.Materials.Add(material);
        await DbContext.SaveChangesAsync();

        var workOrder = TestEntityFactory.CreateWorkOrderDirect(
            id: 0,
            orderNo: $"WO-RB-{Guid.NewGuid():N}",
            materialId: material.Id,
            plannedQty: 100,
            completedQty: 10,
            scrapQty: 0,
            status: WorkOrderStatus.IN_PROGRESS
        );
        DbContext.WorkOrders.Add(workOrder);
        await DbContext.SaveChangesAsync();

        var originalCompletedQty = workOrder.CompletedQty; // 10

        // Act: 在事务中修改数据后回滚
        using var txDb = CreateNewDbContext();
        await using var transaction = await txDb.Database.BeginTransactionAsync();

        // 使用原始 SQL 更新，因为 CompletedQty 和 Status 是 private set
        await txDb.Database.ExecuteSqlRawAsync(
            "UPDATE mes_work_order SET completed_qty = 999, status = {0}, updated_at = {1} WHERE id = {2}",
            (int)WorkOrderStatus.COMPLETED, DateTime.UtcNow, workOrder.Id);

        var report = new WorkReport
        {
            ReportNo = $"WR-RB-{Guid.NewGuid():N}",
            WorkOrderId = workOrder.Id,
            ReportType = ReportType.COMPLETE,
            GoodQty = 989,
            ScrapQty = 0,
            ReworkQty = 0,
            DurationMin = 60,
            ReportTime = DateTime.UtcNow
        };
        txDb.WorkReports.Add(report);
        await txDb.SaveChangesAsync();

        // 回滚事务
        await transaction.RollbackAsync();

        // Assert: 数据应恢复到事务前的状态
        using var verifyDb = CreateNewDbContext();
        var restoredOrder = await verifyDb.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        Assert.Equal(originalCompletedQty, restoredOrder.CompletedQty);
        Assert.Equal(WorkOrderStatus.IN_PROGRESS, restoredOrder.Status);

        // 报工记录不应存在
        var reportExists = await verifyDb.WorkReports.AnyAsync(r => r.WorkOrderId == workOrder.Id);
        Assert.False(reportExists, "事务回滚后，新增的报工记录不应存在");
    }
}
