using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
