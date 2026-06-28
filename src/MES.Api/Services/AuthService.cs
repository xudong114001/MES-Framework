using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Application.Settings;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Api.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<RolePermission> _rolePermissionRepo;
    private readonly JwtSettings _jwt;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<UserRole> userRoleRepo,
        IRepository<Role> roleRepo,
        IRepository<RolePermission> rolePermissionRepo,
        IOptions<JwtSettings> jwt)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _rolePermissionRepo = rolePermissionRepo;
        _jwt = jwt.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _userRepo.FindAsync(u => u.Username == request.Username && !u.IsDeleted);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));
        if (user.PasswordHash != passwordHash) return null;

        // 加载用户角色
        var userRoles = await _userRoleRepo.FindAsync(ur => ur.UserId == user.Id);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _roleRepo.FindAsync(r => roleIds.Contains(r.Id));
        var roleNames = roles.Select(r => r.Name).ToList();

        // 加载用户权限（合并所有角色对应的权限）
        var permissions = await _rolePermissionRepo.FindAsync(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted);
        var permissionList = permissions.Select(rp => rp.Permission).Distinct().ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.DisplayName ?? user.Username),
        };
        claims.AddRange(roleNames.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissionList.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(_jwt.ExpireHours);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        user.LastLoginTime = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            UserInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName ?? user.Username,
                Roles = roleNames,
                Permissions = permissionList
            }
        };
    }
}
