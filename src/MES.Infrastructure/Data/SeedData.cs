using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Infrastructure.Data;

/// <summary>
/// 种子数据配置 - 使用 EF Core HasData 匿名对象机制
/// 利用匿名对象绕过实体 protected set 和 internal 构造函数限制
/// </summary>
public static class SeedData
{
    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        // 角色
        modelBuilder.Entity<Role>().HasData(
            new { Id = 1L, Name = "admin", Description = "系统管理员", CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 2L, Name = "supervisor", Description = "主管", CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 3L, Name = "operator", Description = "操作员", CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 工厂
        modelBuilder.Entity<Factory>().HasData(
            new { Id = 1L, Code = "FACTORY-001", Name = "MES制造基地", Address = "中国上海市嘉定区", Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 车间
        modelBuilder.Entity<Workshop>().HasData(
            new { Id = 1L, Code = "WS-001", Name = "总装车间", FactoryId = 1L, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 产线
        modelBuilder.Entity<ProductionLine>().HasData(
            new { Id = 1L, Code = "LINE-001", Name = "生产线 A", WorkshopId = 1L, LineType = LineType.FLOW, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 工位
        modelBuilder.Entity<Workstation>().HasData(
            new { Id = 1L, Code = "WS-001-01", Name = "工位 1", LineId = 1L, SeqNo = 1, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 2L, Code = "WS-001-02", Name = "工位 2", LineId = 1L, SeqNo = 2, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 3L, Code = "WS-001-03", Name = "工位 3", LineId = 1L, SeqNo = 3, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 物料
        modelBuilder.Entity<Material>().HasData(
            new { Id = 1L, Code = "MAT-001", Name = "产品 A", Spec = "100*50*30", Unit = "PCS", Category = "成品", BomLevel = (int?)null, StockQty = 1000m, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 2L, Code = "MAT-002", Name = "部件 B", Spec = "50*30*10", Unit = "PCS", Category = "半成品", BomLevel = (int?)null, StockQty = 5000m, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false },
            new { Id = 3L, Code = "MAT-003", Name = "螺丝 M6", Spec = "M6*20", Unit = "PCS", Category = "配件", BomLevel = (int?)null, StockQty = 10000m, Status = true, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );

        // 用户 (密码: Admin@2026!)
        modelBuilder.Entity<User>().HasData(
            new { Id = 1L, Username = "admin", PasswordHash = "admin123", DisplayName = "系统管理员", Email = "admin@mes.local", Phone = (string?)null, Status = true, LastLoginTime = (DateTime?)null, CreatedAt = SeedTime, CreatedBy = 0L, UpdatedAt = SeedTime, UpdatedBy = (long?)null, IsDeleted = false }
        );
    }
}
