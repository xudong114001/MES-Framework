using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Application.Settings;
using MES.Api.Services;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using Moq;
using Xunit;

namespace MES.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepo;
    private readonly Mock<IRepository<UserRole>> _userRoleRepo;
    private readonly Mock<IRepository<Role>> _roleRepo;
    private readonly Mock<IRepository<RolePermission>> _rolePermissionRepo;
    private readonly AuthService _service;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _userRepo = new Mock<IRepository<User>>();
        _userRoleRepo = new Mock<IRepository<UserRole>>();
        _roleRepo = new Mock<IRepository<Role>>();
        _rolePermissionRepo = new Mock<IRepository<RolePermission>>();

        _jwtSettings = new JwtSettings
        {
            SecretKey = "Test-Secret-Key-At-Least-32-Characters!@#$",
            Issuer = "Test",
            Audience = "Test",
            ExpireHours = 1
        };

        _service = new AuthService(
            _userRepo.Object,
            _userRoleRepo.Object,
            _roleRepo.Object,
            _rolePermissionRepo.Object,
            Options.Create(_jwtSettings));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Admin@2026!")));

        var user = TestEntityFactory.CreateUserDirect(
            username: "admin",
            displayName: "Admin User",
            passwordHash: passwordHash
        );

        var role = new Role { Name = "Admin" };
        TestEntityFactory.SetProperty(role, "Id", 1L);

        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsEnumerable());
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()))
            .ReturnsAsync(new List<UserRole> { userRole }.AsEnumerable());
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Role, bool>>>()))
            .ReturnsAsync(new List<Role> { role }.AsEnumerable());
        _rolePermissionRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RolePermission, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<RolePermission>());
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var request = new LoginRequest { Username = "admin", Password = "Admin@2026!" };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.NotEqual(default, result.ExpiresAt);
        Assert.NotNull(result.UserInfo);
        Assert.Equal("admin", result.UserInfo.Username);
        Assert.Equal("Admin User", result.UserInfo.DisplayName);
        Assert.Contains("Admin", result.UserInfo.Roles);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        // Arrange
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Admin@2026!")));

        var user = TestEntityFactory.CreateUserDirect(
            username: "admin",
            displayName: "Admin User",
            passwordHash: passwordHash
        );

        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsEnumerable());

        var request = new LoginRequest { Username = "admin", Password = "WrongPassword!" };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        var request = new LoginRequest { Username = "nonexistent", Password = "SomePassword!" };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_DeletedUser_ReturnsNull()
    {
        // Arrange
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Admin@2026!")));

        var user = TestEntityFactory.CreateUserDirect(
            username: "admin",
            displayName: "Admin User"
        );
        TestEntityFactory.SetProperty(user, "PasswordHash", passwordHash);
        user.MarkAsDeleted();

        // Deleted user should not be returned by the repository filter
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        var request = new LoginRequest { Username = "admin", Password = "Admin@2026!" };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }
}
