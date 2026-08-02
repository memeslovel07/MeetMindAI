using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _connection =
            new SqliteConnection("DataSource=:memory:");

        _connection.Open();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

        _context =
            new ApplicationDbContext(options);

        _context.Database.EnsureCreated();

        _repository =
            new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = CreateUser("somesh@example.com");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("somesh@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailExists_ShouldReturnUser()
    {
        // Arrange
        var user = CreateUser("somesh@example.com");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByEmailAsync(
                "somesh@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldNormalizeInput()
    {
        // Arrange
        var user = CreateUser("somesh@example.com");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByEmailAsync(
                "  SoMeSh@Example.Com  ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByEmailAsync(
                "missing@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateUser("somesh@example.com");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.ExistsByEmailAsync(
                user.NormalizedEmail);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result =
            await _repository.ExistsByEmailAsync(
                "MISSING@EXAMPLE.COM");

        // Assert
        Assert.False(result);
    }

    private static User CreateUser(string email)
    {
        var result = User.Create(
            email,
            "test-password-hash",
            "Somesh",
            "Verma");

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldNormalizeInput()
    {
        // Arrange
        var user = CreateUser(
            "somesh@example.com");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.ExistsByEmailAsync(
                "  SoMeSh@Example.Com  ");

        // Assert
        Assert.True(result);
    }
}
