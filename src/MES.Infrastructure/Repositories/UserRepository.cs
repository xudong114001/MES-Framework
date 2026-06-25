using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MesDbContext context) : base(context) { }

    public async Task<bool> ExistsByUsernameAsync(string username, long? excludeId = null)
    {
        var query = _dbSet.Where(u => u.Username == username && !u.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task AssignRolesAsync(long userId, List<string> roleNames)
    {
        var existingRoles = await _context.Set<UserRole>().Where(ur => ur.UserId == userId).ToListAsync();
        _context.Set<UserRole>().RemoveRange(existingRoles);

        foreach (var roleName in roleNames)
        {
            var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDeleted);
            if (role != null)
            {
                _context.Set<UserRole>().Add(new UserRole { UserId = userId, RoleId = role.Id });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetWithRolesAsync(long id)
    {
        return await _dbSet
            .Where(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync();
    }
}
