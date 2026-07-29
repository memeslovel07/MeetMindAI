using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class RefreshTokenRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests()
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
            new RefreshTokenRepository(_context);
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnToken()
    {
        // Arrange
        var user = await CreateAndPersistUserAsync();

        var refreshToken = CreateRefreshToken(
            user.Id,
            "refresh-token-1");

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByTokenAsync(
                "refresh-token-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(refreshToken.Id, result.Id);
        Assert.Equal("refresh-token-1", result.Token);
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldTrimToken()
    {
        // Arrange
        var user = await CreateAndPersistUserAsync();

        var refreshToken = CreateRefreshToken(
            user.Id,
            "refresh-token-trim");

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByTokenAsync(
                "   refresh-token-trim   ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(refreshToken.Id, result.Id);
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByTokenAsync(
                "missing-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldIncludeUser()
    {
        // Arrange
        var user = await CreateAndPersistUserAsync();

        var refreshToken = CreateRefreshToken(
            user.Id,
            "refresh-token-user");

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
            await _repository.GetByTokenAsync(
                "refresh-token-user");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);

        Assert.Equal(
            user.Id,
            result.User.Id);

        Assert.Equal(
            user.Email,
            result.User.Email);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyUsersTokens()
    {
        // Arrange
        var firstUser = await CreateAndPersistUserAsync();
        var secondUser = await CreateAndPersistUserAsync();

        var firstToken = CreateRefreshToken(
            firstUser.Id,
            "first-user-token-1");

        var secondToken = CreateRefreshToken(
            firstUser.Id,
            "first-user-token-2");

        var otherToken = CreateRefreshToken(
            secondUser.Id,
            "second-user-token");

        await _context.RefreshTokens.AddRangeAsync(
            firstToken,
            secondToken,
            otherToken);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByUserIdAsync(
                firstUser.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            token => Assert.Equal(
                firstUser.Id,
                token.UserId));

        Assert.DoesNotContain(
            result,
            token => token.Id == otherToken.Id);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnActiveToken()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        var user = await CreateAndPersistUserAsync();

        var activeToken = RefreshToken.Create(
            user.Id,
            "active-token",
            utcNow.AddDays(7));

        await _context.RefreshTokens.AddAsync(activeToken);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetActiveByUserIdAsync(
                user.Id,
                utcNow);

        // Assert
        Assert.Single(result);
        Assert.Equal(activeToken.Id, result[0].Id);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldExcludeRevokedTokens()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        var user = await CreateAndPersistUserAsync();

        var activeToken = RefreshToken.Create(
            user.Id,
            "active-token",
            utcNow.AddDays(7));

        var revokedToken = RefreshToken.Create(
            user.Id,
            "revoked-token",
            utcNow.AddDays(7));

        revokedToken.Revoke(
            RefreshTokenRevocationReason.UserLogout);

        await _context.RefreshTokens.AddRangeAsync(
            activeToken,
            revokedToken);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetActiveByUserIdAsync(
                user.Id,
                utcNow);

        // Assert
        Assert.Single(result);
        Assert.Equal(activeToken.Id, result[0].Id);

        Assert.DoesNotContain(
            result,
            token => token.Id == revokedToken.Id);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldExcludeExpiredTokens()
    {
        // Arrange
        var user = await CreateAndPersistUserAsync();

        // Create() requires expiration to be in the future
        // at creation time.
        var expiredToken = RefreshToken.Create(
            user.Id,
            "expired-for-query-token",
            DateTime.UtcNow.AddMinutes(5));

        await _context.RefreshTokens.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        var queryTime =
            DateTime.UtcNow.AddMinutes(10);

        // Act
        var result =
            await _repository.GetActiveByUserIdAsync(
                user.Id,
                queryTime);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldExcludeOtherUsersTokens()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        var firstUser = await CreateAndPersistUserAsync();
        var secondUser = await CreateAndPersistUserAsync();

        var firstToken = RefreshToken.Create(
            firstUser.Id,
            "first-active-token",
            utcNow.AddDays(7));

        var secondToken = RefreshToken.Create(
            secondUser.Id,
            "second-active-token",
            utcNow.AddDays(7));

        await _context.RefreshTokens.AddRangeAsync(
            firstToken,
            secondToken);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetActiveByUserIdAsync(
                firstUser.Id,
                utcNow);

        // Assert
        Assert.Single(result);
        Assert.Equal(firstToken.Id, result[0].Id);
    }

    private async Task<User> CreateAndPersistUserAsync()
    {
        var result = User.Create(
            $"refresh-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(result.IsSuccess);

        var user = result.Value;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    private static RefreshToken CreateRefreshToken(
        Guid userId,
        string token)
    {
        return RefreshToken.Create(
            userId,
            token,
            DateTime.UtcNow.AddDays(7));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
