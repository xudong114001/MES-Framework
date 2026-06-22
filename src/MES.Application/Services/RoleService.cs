using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<RolePermission> _permissionRepo;

    public RoleService(IRepository<Role> roleRepo, IRepository<RolePermission> permissionRepo)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        var roles = await _roleRepo.FindAsync(r => !r.IsDeleted);
        return roles.Select(r => new { r.Id, r.Name, r.Description, r.CreatedAt, r.UpdatedAt });
    }

    public async Task<object?> GetByIdAsync(long id)
    {
        var role = (await _roleRepo.FindAsync(r => r.Id == id && !r.IsDeleted)).FirstOrDefault();
        if (role == null) return null;

        var permissions = await _permissionRepo.FindAsync(rp => rp.RoleId == id);
        return new
        {
            role.Id, role.Name, role.Description,
            Permissions = permissions.Select(rp => rp.Permission).ToList(),
            role.CreatedAt, role.UpdatedAt
        };
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(long id)
    {
        var permissions = await _permissionRepo.FindAsync(rp => rp.RoleId == id);
        return permissions.Select(rp => rp.Permission);
    }

    public async Task CreateAsync(string name, string? description)
    {
        if (await _roleRepo.ExistsAsync(r => r.Name == name && !r.IsDeleted))
            throw new Domain.Exceptions.DomainException("角色已存在");

        var role = new Role { Name = name, Description = description };
        await _roleRepo.AddAsync(role);
    }

    public async Task UpdateAsync(long id, string name, string? description)
    {
        var role = (await _roleRepo.FindAsync(r => r.Id == id && !r.IsDeleted)).FirstOrDefault();
        if (role == null)
            throw new Domain.Exceptions.DomainException("角色不存在");

        if (await _roleRepo.ExistsAsync(r => r.Name == name && r.Id != id && !r.IsDeleted))
            throw new Domain.Exceptions.DomainException("角色名称已被使用");

        role.Name = name;
        role.Description = description;
        await _roleRepo.UpdateAsync(role);
    }

    public async Task AssignPermissionsAsync(long id, List<string> permissions)
    {
        var role = (await _roleRepo.FindAsync(r => r.Id == id && !r.IsDeleted)).FirstOrDefault();
        if (role == null)
            throw new Domain.Exceptions.DomainException("角色不存在");

        var existingPermissions = await _permissionRepo.FindAsync(rp => rp.RoleId == id);
        foreach (var perm in existingPermissions)
        {
            await _permissionRepo.DeleteAsync(perm);
        }

        foreach (var perm in permissions.Distinct())
        {
            await _permissionRepo.AddAsync(new RolePermission
            {
                RoleId = id,
                Permission = perm.Trim()
            });
        }
    }

    public async Task DeleteAsync(long id)
    {
        var role = (await _roleRepo.FindAsync(r => r.Id == id && !r.IsDeleted)).FirstOrDefault();
        if (role == null)
            throw new Domain.Exceptions.DomainException("角色不存在");

        // Check if any users have this role
        // Note: UserRole uses IRepository<UserRole> which we'd need to inject or check via other means
        role.IsDeleted = true;
        await _roleRepo.UpdateAsync(role);
    }
}
