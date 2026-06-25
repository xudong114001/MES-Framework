using Microsoft.EntityFrameworkCore;
using MES.AI.Domain.Entities;
using MES.Domain.Entities;
using MES.Infrastructure.Data;
using MES.Tests;
using Xunit;

namespace MES.Tests.Infrastructure;

public class MesDbContextConfigurationTests
{
    [Fact]
    public void WorkOrderEntity_HasRequiredConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new MesDbContext(options);

        // Act
        db.WorkOrders.Add(TestEntityFactory.CreateWorkOrderDirect(
            id: 0,
            orderNo: "WO-TEST-001",
            materialId: 1,
            plannedQty: 100
        ));
        db.SaveChanges();

        var workOrders = db.WorkOrders.ToList();

        // Assert
        Assert.Single(workOrders);
        Assert.Equal("WO-TEST-001", workOrders[0].OrderNo);
        Assert.Equal(100, workOrders[0].PlannedQty.Value);
    }

    [Fact]
    public void RoleEntity_HasConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new MesDbContext(options);

        // Act
        db.Roles.Add(new Role { Name = "test-role" });
        db.SaveChanges();

        var roles = db.Roles.ToList();

        // Assert
        Assert.Single(roles);
        Assert.Equal("test-role", roles[0].Name);
    }

    [Fact]
    public void UserRoleEntity_HasConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new MesDbContext(options);

        var user = new User
        {
            Username = "testuser",
            PasswordHash = "hash",
            DisplayName = "Test User",
            Status = true
        };
        db.Users.Add(user);
        db.SaveChanges();

        var role = new Role { Name = "test-role" };
        db.Roles.Add(role);
        db.SaveChanges();

        // Act
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        db.SaveChanges();

        var userRoles = db.UserRoles.ToList();

        // Assert
        Assert.Single(userRoles);
        Assert.Equal(user.Id, userRoles[0].UserId);
        Assert.Equal(role.Id, userRoles[0].RoleId);
    }

    [Fact]
    public void KnowledgeEntryEntity_CanBeQueried()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new MesDbContext(options);

        // Act
        db.Set<KnowledgeEntry>().Add(new KnowledgeEntry
        {
            Title = "Test Knowledge Entry",
            Content = "This is a test content for knowledge entry.",
            Category = 1
        });
        db.SaveChanges();

        var entries = db.Set<KnowledgeEntry>().ToList();

        // Assert
        Assert.Single(entries);
        Assert.Equal("Test Knowledge Entry", entries[0].Title);
        Assert.Equal("This is a test content for knowledge entry.", entries[0].Content);
    }
}
