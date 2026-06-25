using System.Security.Cryptography;
using System.Text;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<Role> _roleRepo;

    public UserService(
        IUserRepository userRepo,
        IRepository<Role> roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    private static UserDto MapToDto(User entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        DisplayName = entity.DisplayName,
        Email = entity.Email,
        Phone = entity.Phone,
        Status = entity.Status,
        LastLoginTime = entity.LastLoginTime,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var list = await _userRepo.GetAllAsync();
        return list.Where(u => !u.IsDeleted).Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(long id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted) return null;
        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        if (await ExistsAsync(request.Username))
            throw new DomainException("用户名已存在");

        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

        var user = User.Create(
            username: request.Username,
            displayName: request.DisplayName,
            passwordHash: passwordHash,
            email: request.Email,
            phone: request.Phone,
            status: request.Status);

        await _userRepo.AddAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy
        };
    }

    public async Task UpdateAsync(long id, UpdateUserRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new DomainException("用户不存在");

        if (await ExistsAsync(request.Username, id))
            throw new DomainException("用户名已存在");

        user.Username = request.Username;
        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Status = request.Status;

        await _userRepo.UpdateAsync(user);
    }

    public async Task DeleteAsync(long id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new DomainException("用户不存在");

        if (user.Username == "admin")
            throw new DomainException("不能删除超级管理员");

        await _userRepo.DeleteAsync(user);
    }

    public async Task ResetPasswordAsync(long id, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new DomainException("用户不存在");

        user.PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(newPassword)));
        await _userRepo.UpdateAsync(user);
    }

    public async Task AssignRolesAsync(long id, List<string> roles)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new DomainException("用户不存在");

        await _userRepo.AssignRolesAsync(id, roles);
    }

    public async Task<bool> ExistsAsync(string username, long? excludeId = null)
    {
        return await _userRepo.ExistsByUsernameAsync(username, excludeId);
    }
}
