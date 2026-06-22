namespace MES.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<object?> GetByIdAsync(long id);
    Task<IEnumerable<string>> GetPermissionsAsync(long id);
    Task CreateAsync(string name, string? description);
    Task UpdateAsync(long id, string name, string? description);
    Task AssignPermissionsAsync(long id, List<string> permissions);
    Task DeleteAsync(long id);
}
