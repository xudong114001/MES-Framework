using Microsoft.EntityFrameworkCore;
using MES.AI.Application.Dtos;
using MES.AI.Application.Services;
using MES.AI.Domain.Entities;
using MES.Infrastructure.Data;
using MES.Domain.Repositories;
using MES.Infrastructure.Repositories;
using MES.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.AI;

public class KnowledgeBaseServiceTests : IDisposable
{
    private readonly MesDbContext _db;
    private readonly IRepository<KnowledgeEntry> _repo;
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new MesDbContext(options);
        _repo = new Repository<KnowledgeEntry>(_db);
        _service = new KnowledgeBaseService(_repo, Mock.Of<ILogger<KnowledgeBaseService>>());
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    #region SearchAsync

    [Fact]
    public async Task SearchAsync_ByTitle_ReturnsMatch()
    {
        var entry1 = new KnowledgeEntry { Title = "SMT Process Flow", Content = "Some content", Keywords = "smt,process", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Assembly Guide", Content = "Assembly steps", Keywords = "assembly", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("SMT", null);

        Assert.Single(result.Items);
        Assert.Equal("SMT Process Flow", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_ByContent_ReturnsMatch()
    {
        var entry1 = new KnowledgeEntry { Title = "Process A", Content = "This describes SMT soldering process", Keywords = "soldering", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Process B", Content = "General assembly", Keywords = "assembly", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("soldering", null);

        Assert.Single(result.Items);
        Assert.Equal("Process A", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_ByKeywords_ReturnsMatch()
    {
        var entry1 = new KnowledgeEntry { Title = "Process A", Content = "Content A", Keywords = "smt,reflow,profile", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Process B", Content = "Content B", Keywords = "assembly", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("reflow", null);

        Assert.Single(result.Items);
        Assert.Equal("Process A", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ReturnsFiltered()
    {
        var entry1 = new KnowledgeEntry { Title = "Process Standard Doc", Content = "Content", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Quality Spec Doc", Content = "Content", Keywords = "", Category = 1 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync(null, 0);

        Assert.Single(result.Items);
        Assert.Equal("Process Standard Doc", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_WithNoMatch_ReturnsEmpty()
    {
        var entry = new KnowledgeEntry { Title = "Process A", Content = "Content A", Keywords = "smt", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 1);

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("nonexistent", null);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsAll()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "Content 1", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "Content 2", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("", null);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "A", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        TestEntityFactory.SetProperty(entry1, "CreatedAt", new DateTime(2024, 1, 3));
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "B", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);
        TestEntityFactory.SetProperty(entry2, "CreatedAt", new DateTime(2024, 1, 2));
        var entry3 = new KnowledgeEntry { Title = "Doc 3", Content = "C", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry3, "Id", 3);
        TestEntityFactory.SetProperty(entry3, "CreatedAt", new DateTime(2024, 1, 1));

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2, entry3);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync(null, null, page: 2, pageSize: 1);

        Assert.Single(result.Items);
        Assert.Equal("Doc 2", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_SkipsDeletedEntries()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "Content 1", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        entry1.MarkAsDeleted();
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "Content 2", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.SearchAsync("", null);

        Assert.Single(result.Items);
        Assert.Equal("Doc 2", result.Items[0].Title);
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WithoutCategory_ReturnsAll()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "Content 1", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "Content 2", Keywords = "", Category = 1 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithCategory_ReturnsFiltered()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "Content 1", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        TestEntityFactory.SetProperty(entry1, "CreatedAt", new DateTime(2024, 1, 2));
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "Content 2", Keywords = "", Category = 1 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);
        TestEntityFactory.SetProperty(entry2, "CreatedAt", new DateTime(2024, 1, 1));
        var entry3 = new KnowledgeEntry { Title = "Doc 3", Content = "Content 3", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry3, "Id", 3);
        TestEntityFactory.SetProperty(entry3, "CreatedAt", new DateTime(2024, 1, 3));

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2, entry3);
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(0);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Title == "Doc 1");
        Assert.Contains(result, d => d.Title == "Doc 3");
    }

    [Fact]
    public async Task GetAllAsync_SkipsDeletedEntries()
    {
        var entry1 = new KnowledgeEntry { Title = "Doc 1", Content = "Content 1", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry1, "Id", 1);
        entry1.MarkAsDeleted();
        var entry2 = new KnowledgeEntry { Title = "Doc 2", Content = "Content 2", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry2, "Id", 2);

        _db.Set<KnowledgeEntry>().AddRange(entry1, entry2);
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(null);

        Assert.Single(result);
        Assert.Equal("Doc 2", result[0].Title);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsDto()
    {
        var entry = new KnowledgeEntry { Title = "Found Doc", Content = "Content", Keywords = "key", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 42);

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var result = await _service.GetByIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Found Doc", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(99);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedEntry_ReturnsNull()
    {
        var entry = new KnowledgeEntry { Title = "Deleted Doc", Content = "Content", Keywords = "key", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 10);
        entry.MarkAsDeleted();

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var result = await _service.GetByIdAsync(10);
        Assert.Null(result);
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_CreatesEntity_ReturnsDto()
    {
        var dto = new KnowledgeEntryDto
        {
            Title = "New Entry",
            Content = "New Content",
            Keywords = "new,entry",
            Category = 1,
            MaterialId = 100,
            EquipmentId = 200
        };

        var result = await _service.AddAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("New Entry", result.Title);
        Assert.Equal("New Content", result.Content);
        Assert.Equal(1, result.Category);

        var saved = await _db.Set<KnowledgeEntry>().FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Entry", saved.Title);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_Existing_UpdatesFields()
    {
        var entry = new KnowledgeEntry { Title = "Old Title", Content = "Old Content", Keywords = "old", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 5);
        TestEntityFactory.SetProperty(entry, "CreatedAt", DateTime.UtcNow.AddDays(-1));

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var updateDto = new KnowledgeEntryDto { Title = "Updated Title", Content = "Updated Content", Keywords = "updated", Category = 2, MaterialId = 99, EquipmentId = 199 };

        var result = await _service.UpdateAsync(5, updateDto);

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Content", result.Content);
    }

    [Fact]
    public async Task UpdateAsync_NonExisting_ReturnsNull()
    {
        var dto = new KnowledgeEntryDto { Title = "N/A", Content = "N/A" };
        var result = await _service.UpdateAsync(99, dto);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_DeletedEntry_ReturnsNull()
    {
        var entry = new KnowledgeEntry { Title = "Deleted", Content = "Content", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 7);
        entry.MarkAsDeleted();

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var dto = new KnowledgeEntryDto { Title = "Update", Content = "N/A" };
        var result = await _service.UpdateAsync(7, dto);
        Assert.Null(result);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_Existing_SoftDeletes()
    {
        var entry = new KnowledgeEntry { Title = "To Delete", Content = "Content", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 3);

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteAsync(3);

        Assert.True(result);

        var entity = await _db.Set<KnowledgeEntry>().FirstOrDefaultAsync(e => e.Id == 3);
        Assert.NotNull(entity);
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(99);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeleted_ReturnsFalse()
    {
        var entry = new KnowledgeEntry { Title = "Already Deleted", Content = "Content", Keywords = "", Category = 0 };
        TestEntityFactory.SetProperty(entry, "Id", 4);
        entry.MarkAsDeleted();

        _db.Set<KnowledgeEntry>().Add(entry);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteAsync(4);
        Assert.False(result);
    }

    #endregion
}
