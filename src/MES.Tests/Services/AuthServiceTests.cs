using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MES.Api.Dtos;
using MES.Api.Services;
using MES.Domain.Entities;
using MES.Infrastructure.Data;
using Xunit;

namespace MES.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly MesDbContext _db;
    private readonly AuthService _service;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new MesDbContext(options);

        _jwtSettings = new JwtSettings
        {
            SecretKey = "Test-Secret-Key-At-Least-32-Characters!@#$",
            Issuer = "Test",
            Audience = "Test",
            ExpireHours = 1
        };

        _service = new AuthService(_db, Options.Create(_jwtSettings));
    }

    public void Dispose()
    {
        _db.Dispose();
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
        _db.Users.Add(user);
        _db.SaveChanges();

        var role = new Role { Name = "admin" };
        _db.Roles.Add(role);
        _db.SaveChanges();

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        _db.SaveChanges();

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
        Assert.Contains("admin", result.UserInfo.Roles);
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
        _db.Users.Add(user);
        _db.SaveChanges();

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

        _db.Users.Add(user);
        _db.SaveChanges();

        var request = new LoginRequest { Username = "admin", Password = "Admin@2026!" };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }
}
