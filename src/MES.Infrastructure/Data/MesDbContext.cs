using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;
using MES.Domain.Events;

namespace MES.Infrastructure.Data;

public class MesDbContext : DbContext
{
    public MesDbContext(DbContextOptions<MesDbContext> options) : base(options) { }

    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Bom> Boms => Set<Bom>();
    public DbSet<Routing> Routings => Set<Routing>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderStep> WorkOrderSteps => Set<WorkOrderStep>();
    public DbSet<WorkReport> WorkReports => Set<WorkReport>();
    public DbSet<QcInspection> QcInspections => Set<QcInspection>();
    public DbSet<QcInspectionItem> QcInspectionItems => Set<QcInspectionItem>();
    public DbSet<MaterialTrace> MaterialTraces => Set<MaterialTrace>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AlertRecord> AlertRecords => Set<AlertRecord>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<KnowledgeEntry> KnowledgeEntries => Set<KnowledgeEntry>();
    public DbSet<AndonEvent> AndonEvents => Set<AndonEvent>();
    public DbSet<QcCheckpoint> QcCheckpoints => Set<QcCheckpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DomainEvent 是领域事件基类，不应被 EF Core 持久化
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MesDbContext).Assembly);

        // 应用种子数据配置
        SeedData.ConfigureSeedData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreationInfo(DateTime.UtcNow);
                    entry.Entity.SetModificationInfo(DateTime.UtcNow);
                    break;
                case EntityState.Modified:
                    entry.Entity.SetModificationInfo(DateTime.UtcNow);
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
