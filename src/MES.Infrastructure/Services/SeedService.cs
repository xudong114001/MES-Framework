using System.Security.Cryptography;
using System.Text;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Interfaces;
using MES.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MES.Infrastructure.Services;

public class SeedService : ISeedService
{
    private readonly MesDbContext _db;

    public SeedService(MesDbContext db) => _db = db;

    private sealed record SeedStats
    {
        public int Factories { get; set; }
        public int Workshops { get; set; }
        public int ProductionLines { get; set; }
        public int Workstations { get; set; }
        public int Materials { get; set; }
        public int Boms { get; set; }
        public int Routings { get; set; }
        public int RoutingSteps { get; set; }
        public int WorkOrders { get; set; }
        public int Users { get; set; }
    }

    public async Task<object> SeedAsync()
    {
        var stats = new SeedStats();

        await using var tx = await _db.Database.BeginTransactionAsync();

        // ── 1. 工厂 ──────────────────────────────────────────
        var factory = await _db.Factories.FirstOrDefaultAsync(f => f.Code == "FACTORY-001");
        if (factory == null)
        {
            factory = new Factory
            {
                Code = "FACTORY-001",
                Name = "Demo Factory",
                Address = "中国广东省深圳市南山区科技园",
                Status = true
            };
            _db.Factories.Add(factory);
            await _db.SaveChangesAsync();
        }
        stats.Factories = 1;

        // ── 2. 车间 ──────────────────────────────────────────
        var wsSmt = await _db.Workshops.FirstOrDefaultAsync(w => w.Code == "WS-SMT");
        if (wsSmt == null)
        {
            wsSmt = new Workshop { FactoryId = factory.Id, Code = "WS-SMT", Name = "SMT车间", Status = true };
            _db.Workshops.Add(wsSmt);
            await _db.SaveChangesAsync();
            stats.Workshops++;
        }

        var wsAssembly = await _db.Workshops.FirstOrDefaultAsync(w => w.Code == "WS-ASSEMBLY");
        if (wsAssembly == null)
        {
            wsAssembly = new Workshop { FactoryId = factory.Id, Code = "WS-ASSEMBLY", Name = "组装车间", Status = true };
            _db.Workshops.Add(wsAssembly);
            await _db.SaveChangesAsync();
            stats.Workshops++;
        }

        // ── 3. 产线 ──────────────────────────────────────────
        var smt01 = await _db.ProductionLines.FirstOrDefaultAsync(p => p.Code == "SMT-01");
        if (smt01 == null)
        {
            smt01 = new ProductionLine { WorkshopId = wsSmt.Id, Code = "SMT-01", Name = "SMT-01 贴片线", LineType = LineType.FLOW, Status = true };
            _db.ProductionLines.Add(smt01);
            await _db.SaveChangesAsync();
            stats.ProductionLines++;
        }

        var smt02 = await _db.ProductionLines.FirstOrDefaultAsync(p => p.Code == "SMT-02");
        if (smt02 == null)
        {
            smt02 = new ProductionLine { WorkshopId = wsSmt.Id, Code = "SMT-02", Name = "SMT-02 贴片线", LineType = LineType.FLOW, Status = true };
            _db.ProductionLines.Add(smt02);
            await _db.SaveChangesAsync();
            stats.ProductionLines++;
        }

        var assy01 = await _db.ProductionLines.FirstOrDefaultAsync(p => p.Code == "ASSY-01");
        if (assy01 == null)
        {
            assy01 = new ProductionLine { WorkshopId = wsAssembly.Id, Code = "ASSY-01", Name = "ASSY-01 组装线", LineType = LineType.CELL, Status = true };
            _db.ProductionLines.Add(assy01);
            await _db.SaveChangesAsync();
            stats.ProductionLines++;
        }

        var assy02 = await _db.ProductionLines.FirstOrDefaultAsync(p => p.Code == "ASSY-02");
        if (assy02 == null)
        {
            assy02 = new ProductionLine { WorkshopId = wsAssembly.Id, Code = "ASSY-02", Name = "ASSY-02 组装线", LineType = LineType.CELL, Status = true };
            _db.ProductionLines.Add(assy02);
            await _db.SaveChangesAsync();
            stats.ProductionLines++;
        }

        // ── 4. 工位 ──────────────────────────────────────────
        // SMT-01: 上板机 → 印刷机 → 贴片机 → 回流焊 → AOI
        stats.Workstations += await CreateWorkstationIfNotExist(smt01.Id, "SMT01-LOADER", "上板机", 10);
        stats.Workstations += await CreateWorkstationIfNotExist(smt01.Id, "SMT01-PRINTER", "锡膏印刷机", 20);
        stats.Workstations += await CreateWorkstationIfNotExist(smt01.Id, "SMT01-PLACER", "高速贴片机", 30);
        stats.Workstations += await CreateWorkstationIfNotExist(smt01.Id, "SMT01-REFLOW", "回流焊炉", 40);
        stats.Workstations += await CreateWorkstationIfNotExist(smt01.Id, "SMT01-AOI", "AOI检测机", 50);

        // SMT-02: 上板机 → 印刷机 → 贴片机 → 回流焊
        stats.Workstations += await CreateWorkstationIfNotExist(smt02.Id, "SMT02-LOADER", "上板机", 10);
        stats.Workstations += await CreateWorkstationIfNotExist(smt02.Id, "SMT02-PRINTER", "锡膏印刷机", 20);
        stats.Workstations += await CreateWorkstationIfNotExist(smt02.Id, "SMT02-PLACER", "高速贴片机", 30);
        stats.Workstations += await CreateWorkstationIfNotExist(smt02.Id, "SMT02-REFLOW", "回流焊炉", 40);

        // ASSY-01: 手工插件 → 波峰焊 → 测试
        stats.Workstations += await CreateWorkstationIfNotExist(assy01.Id, "ASSY01-TH", "手工插件工位", 10);
        stats.Workstations += await CreateWorkstationIfNotExist(assy01.Id, "ASSY01-WAVE", "波峰焊", 20);
        stats.Workstations += await CreateWorkstationIfNotExist(assy01.Id, "ASSY01-TEST", "功能测试", 30);

        // ASSY-02: 组装 → 老化 → 包装
        stats.Workstations += await CreateWorkstationIfNotExist(assy02.Id, "ASSY02-ASSEMBLE", "整机组装", 10);
        stats.Workstations += await CreateWorkstationIfNotExist(assy02.Id, "ASSY02-BURNIN", "老化测试", 20);
        stats.Workstations += await CreateWorkstationIfNotExist(assy02.Id, "ASSY02-PACK", "包装工位", 30);

        // ── 5. 物料 ──────────────────────────────────────────
        // 3个成品
        var fgA001_r = await CreateMaterialIfNotExist("FG-A001", "PCBA板", "V1.0", "PCS", "FG", 0, 0);
        var fgA002_r = await CreateMaterialIfNotExist("FG-A002", "成品模组", "V1.0", "PCS", "FG", 1, 0);
        var fgA003_r = await CreateMaterialIfNotExist("FG-A003", "整机", "V1.0", "PCS", "FG", 2, 0);
        var ic100_r = await CreateMaterialIfNotExist("IC-100", "主控芯片", "STM32F407", "PCS", "RM", 0, 5000);
        var res101_r = await CreateMaterialIfNotExist("RES-101", "贴片电阻", "10KΩ 0805", "PCS", "RM", 0, 50000);
        var cap201_r = await CreateMaterialIfNotExist("CAP-201", "电容", "100μF 16V", "PCS", "RM", 0, 20000);
        var pcbA01_r = await CreateMaterialIfNotExist("PCB-A01", "电路板", "FR-4 四层板 100x80mm", "PCS", "RM", 0, 3000);
        var cab001_r = await CreateMaterialIfNotExist("CAB-001", "电源线", "DC 5V 2A 1.5m", "PCS", "RM", 0, 10000);

        var fgA001 = fgA001_r.Entity;
        var fgA002 = fgA002_r.Entity;
        var fgA003 = fgA003_r.Entity;
        var ic100 = ic100_r.Entity;
        var res101 = res101_r.Entity;
        var cap201 = cap201_r.Entity;
        var pcbA01 = pcbA01_r.Entity;
        var cab001 = cab001_r.Entity;
        stats.Materials = (fgA001_r.Created ? 1 : 0) + (fgA002_r.Created ? 1 : 0) + (fgA003_r.Created ? 1 : 0)
            + (ic100_r.Created ? 1 : 0) + (res101_r.Created ? 1 : 0) + (cap201_r.Created ? 1 : 0)
            + (pcbA01_r.Created ? 1 : 0) + (cab001_r.Created ? 1 : 0);

        // ── 6. BOM 清单 ──────────────────────────────────────
        // FG-A001: 需要 IC-100(2) + RES-101(20) + CAP-201(10) + PCB-A01(1)
        stats.Boms += await CreateBomIfNotExist(fgA001.Id, ic100.Id, 2, 0.01m, 10);
        stats.Boms += await CreateBomIfNotExist(fgA001.Id, res101.Id, 20, 0.02m, 20);
        stats.Boms += await CreateBomIfNotExist(fgA001.Id, cap201.Id, 10, 0.01m, 30);
        stats.Boms += await CreateBomIfNotExist(fgA001.Id, pcbA01.Id, 1, 0.005m, 40);

        // ── 7. 工艺路线 ──────────────────────────────────────
        // FG-A001 的 SMT 路线
        var routing = await _db.Routings.FirstOrDefaultAsync(r => r.RoutingCode == "R-SMT-FGA001");
        if (routing == null)
        {
            routing = Routing.Create(fgA001.Id, "R-SMT-FGA001", "PCBA SMT贴片工艺");
            _db.Routings.Add(routing);
            await _db.SaveChangesAsync();
            stats.Routings = 1;

            // 步骤：上板→涂锡→贴片→回流焊→AOI检测
            var steps = new List<RoutingStep>
            {
                RoutingStep.Create(routing.Id, "上板", 10, 0.5m),
                RoutingStep.Create(routing.Id, "涂锡", 20, 1.0m),
                RoutingStep.Create(routing.Id, "贴片", 30, 3.0m),
                RoutingStep.Create(routing.Id, "回流焊", 40, 2.0m),
                RoutingStep.Create(routing.Id, "AOI检测", 50, 1.0m, isQcPoint: true),
            };
            _db.RoutingSteps.AddRange(steps);
            await _db.SaveChangesAsync();
            stats.RoutingSteps = steps.Count;
        }
        else
        {
            stats.Routings = 1;
            stats.RoutingSteps = await _db.RoutingSteps.CountAsync(rs => rs.RoutingId == routing.Id);
        }

        var routingAssy = await _db.Routings.FirstOrDefaultAsync(r => r.RoutingCode == "R-ASSY-FGA002");
        if (routingAssy == null)
        {
            routingAssy = Routing.Create(fgA002.Id, "R-ASSY-FGA002", "成品模组组装工艺");
            _db.Routings.Add(routingAssy);
            await _db.SaveChangesAsync();
            stats.Routings++;

            var stepsAssy = new List<RoutingStep>
            {
                RoutingStep.Create(routingAssy.Id, "手工插件", 10, 2.0m),
                RoutingStep.Create(routingAssy.Id, "波峰焊", 20, 1.5m),
                RoutingStep.Create(routingAssy.Id, "功能测试", 30, 1.0m, isQcPoint: true),
            };
            _db.RoutingSteps.AddRange(stepsAssy);
            await _db.SaveChangesAsync();
            stats.RoutingSteps += stepsAssy.Count;
        }
        else
        {
            stats.Routings++;
            stats.RoutingSteps += await _db.RoutingSteps.CountAsync(rs => rs.RoutingId == routingAssy.Id);
        }

        var routingFinal = await _db.Routings.FirstOrDefaultAsync(r => r.RoutingCode == "R-FINAL-FGA003");
        if (routingFinal == null)
        {
            routingFinal = Routing.Create(fgA003.Id, "R-FINAL-FGA003", "整机总装工艺");
            _db.Routings.Add(routingFinal);
            await _db.SaveChangesAsync();
            stats.Routings++;

            var stepsFinal = new List<RoutingStep>
            {
                RoutingStep.Create(routingFinal.Id, "整机组装", 10, 3.0m),
                RoutingStep.Create(routingFinal.Id, "老化测试", 20, 4.0m, isQcPoint: true),
                RoutingStep.Create(routingFinal.Id, "包装", 30, 1.0m),
            };
            _db.RoutingSteps.AddRange(stepsFinal);
            await _db.SaveChangesAsync();
            stats.RoutingSteps += stepsFinal.Count;
        }
        else
        {
            stats.Routings++;
            stats.RoutingSteps += await _db.RoutingSteps.CountAsync(rs => rs.RoutingId == routingFinal.Id);
        }

        // ── 8. 测试工单 ──────────────────────────────────────
        var now = DateTime.UtcNow;

        stats.WorkOrders += await CreateWorkOrderIfNotExist("WO-20260511-001",
            () => WorkOrder.Create(
                "WO-20260511-001",
                SourceType.MANUAL,
                fgA001.Id,
                100,
                Priority.NORMAL,
                routing.Id,
                null,
                now.AddDays(1),
                now.AddDays(3),
                factory.Id,
                wsSmt.Id,
                smt01.Id,
                null,
                "PCBA板试产工单"));

        stats.WorkOrders += await CreateWorkOrderIfNotExist("WO-20260511-002",
            () => WorkOrder.Create(
                "WO-20260511-002",
                SourceType.MANUAL,
                fgA002.Id,
                50,
                Priority.NORMAL,
                routingAssy.Id,
                null,
                now.AddDays(2),
                now.AddDays(5),
                factory.Id,
                wsAssembly.Id,
                assy01.Id,
                null,
                "成品模组试产工单"));

        stats.WorkOrders += await CreateWorkOrderIfNotExist("WO-20260511-003",
            () => WorkOrder.Create(
                "WO-20260511-003",
                SourceType.MANUAL,
                fgA003.Id,
                200,
                Priority.NORMAL,
                routingFinal.Id,
                null,
                now.AddDays(3),
                now.AddDays(7),
                factory.Id,
                wsAssembly.Id,
                assy02.Id,
                null,
                "整机试产工单"));

        // ── 9. 操作员用户 ──────────────────────────────────────
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == "operator");
        if (existingUser == null)
        {
            var pwHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("123456")));
            _db.Users.Add(Domain.Entities.User.Create(
                username: "operator",
                displayName: "操作员",
                passwordHash: pwHash,
                email: "operator@demo.com"
            ));
            await _db.SaveChangesAsync();
            stats.Users = 1;
        }
        else
        {
            stats.Users = 1; // already exists
        }

        await tx.CommitAsync();

        return stats;
    }

    private async Task<int> CreateWorkstationIfNotExist(long lineId, string code, string name, int seqNo)
    {
        var exists = await _db.Workstations.AnyAsync(w => w.Code == code);
        if (exists) return 0;
        _db.Workstations.Add(Workstation.Create(lineId, code, name, seqNo));
        await _db.SaveChangesAsync();
        return 1;
    }

    private async Task<(Material Entity, bool Created)> CreateMaterialIfNotExist(string code, string name, string spec, string unit, string category, int bomLevel, decimal stockQty)
    {
        var existing = await _db.Materials.FirstOrDefaultAsync(m => m.Code == code);
        if (existing != null) return (existing, false);
        var entity = new Material { Code = code, Name = name, Spec = spec, Unit = unit, Category = category, BomLevel = bomLevel, StockQty = stockQty, Status = true };
        _db.Materials.Add(entity);
        await _db.SaveChangesAsync();
        return (entity, true);
    }

    private async Task<int> CreateBomIfNotExist(long productId, long materialId, decimal quantity, decimal scrapRate, int seqNo)
    {
        var exists = await _db.Boms.AnyAsync(b => b.ProductId == productId && b.MaterialId == materialId);
        if (exists) return 0;
        _db.Boms.Add(new Bom
        {
            ProductId = productId,
            MaterialId = materialId,
            Quantity = quantity,
            ScrapRate = scrapRate,
            SeqNo = seqNo,
            ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = true
        });
        await _db.SaveChangesAsync();
        return 1;
    }

    private async Task<int> CreateWorkOrderIfNotExist(string orderNo, Func<WorkOrder> factory)
    {
        var exists = await _db.WorkOrders.AnyAsync(wo => wo.OrderNo == orderNo);
        if (exists) return 0;
        _db.WorkOrders.Add(factory());
        await _db.SaveChangesAsync();
        return 1;
    }
}
