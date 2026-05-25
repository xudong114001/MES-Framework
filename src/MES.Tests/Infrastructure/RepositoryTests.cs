using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Data;
using MES.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MES.Tests.Infrastructure;

/// <summary>
/// 泛型仓储 Repository&lt;T&gt; 的集成测试。
/// 使用 EF Core InMemory 数据库验证 CRUD 及条件查询操作的真实行为。
/// </summary>
public class RepositoryTests
{
    /// <summary>
    /// 创建独立的 InMemory 数据库上下文，每个测试方法使用独立的数据库实例，
    /// 避免测试间的数据干扰。
    /// </summary>
    private static MesDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MesDbContext(options);
    }

    #region AddAsync

    /// <summary>
    /// 验证 AddAsync 能将实体正确持久化到数据库，并返回包含自动生成 Id 的实体。
    /// </summary>
    [Fact]
    public async Task AddAsync_ValidEntity_PersistsAndReturnsEntity()
    {
        // Arrange
        var dbName = $"RepoTest_AddAsync_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        var factory = new Factory
        {
            Code = "F001",
            Name = "Test Factory",
            Address = "123 Main St"
        };

        // Act
        var result = await repo.AddAsync(factory);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);

        // 验证数据已持久化
        using var assertContext = CreateContext(dbName);
        var saved = await assertContext.Factories.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("F001", saved!.Code);
        Assert.Equal("Test Factory", saved.Name);
    }

    /// <summary>
    /// 验证 AddAsync 多个实体后，数据库中记录数正确。
    /// </summary>
    [Fact]
    public async Task AddAsync_MultipleEntities_AllPersisted()
    {
        // Arrange
        var dbName = $"RepoTest_AddMultiple_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Material>(context);

        var material1 = new Material { Code = "M001", Name = "Material A" };
        var material2 = new Material { Code = "M002", Name = "Material B" };

        // Act
        await repo.AddAsync(material1);
        await repo.AddAsync(material2);

        // Assert
        using var assertContext = CreateContext(dbName);
        var count = await assertContext.Materials.CountAsync();
        Assert.Equal(2, count);
    }

    #endregion

    #region GetByIdAsync

    /// <summary>
    /// 验证 GetByIdAsync 能根据 Id 正确获取已存在的实体。
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        // Arrange
        var dbName = $"RepoTest_GetById_Existing_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        var factory = new Factory { Code = "F010", Name = "FindMe Factory" };
        var added = await repo.AddAsync(factory);

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<Factory>(queryContext);
        var result = await queryRepo.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(added.Id, result!.Id);
        Assert.Equal("F010", result.Code);
    }

    /// <summary>
    /// 验证 GetByIdAsync 查询不存在的 Id 时返回 null。
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var dbName = $"RepoTest_GetById_NonExist_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        // Act
        var result = await repo.GetByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateAsync

    /// <summary>
    /// 验证 UpdateAsync 能正确更新实体的属性值到数据库。
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ModifiedEntity_PersistsChanges()
    {
        // Arrange
        var dbName = $"RepoTest_UpdateAsync_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        var factory = new Factory { Code = "F020", Name = "Original Name" };
        var added = await repo.AddAsync(factory);

        // Act
        using var updateContext = CreateContext(dbName);
        var updateRepo = new Repository<Factory>(updateContext);
        var toUpdate = (await updateRepo.GetByIdAsync(added.Id))!;
        toUpdate.Name = "Updated Name";
        toUpdate.Address = "New Address";
        await updateRepo.UpdateAsync(toUpdate);

        // Assert
        using var assertContext = CreateContext(dbName);
        var updated = await assertContext.Factories.FindAsync(added.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal("New Address", updated.Address);
        Assert.Equal("F020", updated.Code); // 未修改的字段保持不变
    }

    #endregion

    #region DeleteAsync

    /// <summary>
    /// 验证 DeleteAsync 能从数据库中物理删除实体。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var dbName = $"RepoTest_DeleteAsync_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        var factory = new Factory { Code = "F030", Name = "DeleteMe Factory" };
        var added = await repo.AddAsync(factory);

        // Act
        using var deleteContext = CreateContext(dbName);
        var deleteRepo = new Repository<Factory>(deleteContext);
        var toDelete = (await deleteRepo.GetByIdAsync(added.Id))!;
        await deleteRepo.DeleteAsync(toDelete);

        // Assert
        using var assertContext = CreateContext(dbName);
        var deleted = await assertContext.Factories.FindAsync(added.Id);
        Assert.Null(deleted);
    }

    /// <summary>
    /// 验证删除实体后，数据库中记录数正确减少。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_OneOfTwoEntities_CountDecreases()
    {
        // Arrange
        var dbName = $"RepoTest_DeleteCount_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Material>(context);

        var m1 = new Material { Code = "M100", Name = "Keep" };
        var m2 = new Material { Code = "M200", Name = "Remove" };
        await repo.AddAsync(m1);
        await repo.AddAsync(m2);

        // Act
        using var deleteContext = CreateContext(dbName);
        var deleteRepo = new Repository<Material>(deleteContext);
        var toRemove = (await deleteRepo.FindAsync(m => m.Code == "M200")).First();
        await deleteRepo.DeleteAsync(toRemove);

        // Assert
        using var assertContext = CreateContext(dbName);
        var remaining = await assertContext.Materials.ToListAsync();
        Assert.Single(remaining);
        Assert.Equal("M100", remaining[0].Code);
    }

    #endregion

    #region FindAsync

    /// <summary>
    /// 验证 FindAsync 能根据条件表达式正确筛选实体。
    /// </summary>
    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        var dbName = $"RepoTest_FindAsync_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Material>(context);

        await repo.AddAsync(new Material { Code = "M-A01", Name = "Aluminum", Category = "Metal" });
        await repo.AddAsync(new Material { Code = "M-P01", Name = "Plastic", Category = "Polymer" });
        await repo.AddAsync(new Material { Code = "M-A02", Name = "Steel", Category = "Metal" });

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<Material>(queryContext);
        var metals = await queryRepo.FindAsync(m => m.Category == "Metal");

        // Assert
        Assert.Equal(2, metals.Count());
        Assert.All(metals, m => Assert.Equal("Metal", m.Category));
    }

    /// <summary>
    /// 验证 FindAsync 无匹配条件时返回空集合。
    /// </summary>
    [Fact]
    public async Task FindAsync_NoMatch_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = $"RepoTest_FindNoMatch_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Material>(context);

        await repo.AddAsync(new Material { Code = "M-X01", Name = "Something" });

        // Act
        var results = await repo.FindAsync(m => m.Code == "NOT_EXIST");

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// 验证 FindAsync 支持组合条件查询。
    /// </summary>
    [Fact]
    public async Task FindAsync_CombinedPredicate_ReturnsCorrectResults()
    {
        // Arrange
        var dbName = $"RepoTest_FindCombined_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        await repo.AddAsync(new Factory { Code = "F-A", Name = "Active Factory", Status = true });
        await repo.AddAsync(new Factory { Code = "F-B", Name = "Inactive Factory", Status = false });
        await repo.AddAsync(new Factory { Code = "F-C", Name = "Another Active", Status = true });

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<Factory>(queryContext);
        var activeWithCodeA = await queryRepo.FindAsync(f => f.Status && f.Code.StartsWith("F-A"));

        // Assert
        Assert.Single(activeWithCodeA);
        Assert.Equal("F-A", activeWithCodeA.First().Code);
    }

    #endregion

    #region ExistsAsync / CountAsync / GetAllAsync

    /// <summary>
    /// 验证 ExistsAsync 能正确判断满足条件的实体是否存在。
    /// </summary>
    [Fact]
    public async Task ExistsAsync_MatchingEntity_ReturnsTrue()
    {
        // Arrange
        var dbName = $"RepoTest_Exists_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<User>(context);

        await repo.AddAsync(new User { Username = "admin", DisplayName = "Admin" });

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<User>(queryContext);
        var exists = await queryRepo.ExistsAsync(u => u.Username == "admin");
        var notExists = await queryRepo.ExistsAsync(u => u.Username == "guest");

        // Assert
        Assert.True(exists);
        Assert.False(notExists);
    }

    /// <summary>
    /// 验证 CountAsync 能正确统计实体数量，支持条件过滤和全量统计。
    /// </summary>
    [Fact]
    public async Task CountAsync_WithAndWithoutPredicate_ReturnsCorrectCount()
    {
        // Arrange
        var dbName = $"RepoTest_Count_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Factory>(context);

        await repo.AddAsync(new Factory { Code = "F1", Name = "A", Status = true });
        await repo.AddAsync(new Factory { Code = "F2", Name = "B", Status = false });
        await repo.AddAsync(new Factory { Code = "F3", Name = "C", Status = true });

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<Factory>(queryContext);
        var totalCount = await queryRepo.CountAsync();
        var activeCount = await queryRepo.CountAsync(f => f.Status);

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal(2, activeCount);
    }

    /// <summary>
    /// 验证 GetAllAsync 返回所有实体。
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var dbName = $"RepoTest_GetAll_{Guid.NewGuid()}";
        using var context = CreateContext(dbName);
        var repo = new Repository<Material>(context);

        await repo.AddAsync(new Material { Code = "M1", Name = "A" });
        await repo.AddAsync(new Material { Code = "M2", Name = "B" });

        // Act
        using var queryContext = CreateContext(dbName);
        var queryRepo = new Repository<Material>(queryContext);
        var all = await queryRepo.GetAllAsync();

        // Assert
        Assert.Equal(2, all.Count());
    }

    #endregion
}
