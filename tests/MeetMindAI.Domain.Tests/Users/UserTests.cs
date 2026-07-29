using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;

namespace MeetMindAI.Domain.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveUser()
    {
        // Act
        var result = User.Create(
            "somesh@example.com",
            "hashed-password",
            "Somesh",
            "Verma");

        // Assert
        Assert.True(result.IsSuccess);

        var user = result.Value;

        Assert.Equal("somesh@example.com", user.Email);
        Assert.Equal("SOMESH@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal("Somesh", user.FirstName);
        Assert.Equal("Verma", user.LastName);

        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal(EntityStatus.Active, user.Status);

        Assert.False(user.EmailConfirmed);
        Assert.Null(user.LastLoginAtUtc);
        Assert.Null(user.AvatarUrl);
    }

    [Fact]
    public void Create_ShouldTrimUserInformation()
    {
        var result = User.Create(
            "  somesh@example.com  ",
            "hashed-password",
            "  Somesh  ",
            "  Verma  ");

        Assert.True(result.IsSuccess);

        Assert.Equal(
            "somesh@example.com",
            result.Value.Email);

        Assert.Equal(
            "SOMESH@EXAMPLE.COM",
            result.Value.NormalizedEmail);

        Assert.Equal(
            "Somesh",
            result.Value.FirstName);

        Assert.Equal(
            "Verma",
            result.Value.LastName);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldFail()
    {
        var result = User.Create(
            "   ",
            "hashed-password",
            "Somesh",
            "Verma");

        Assert.True(result.IsFailure);
        Assert.Equal(
            UserErrors.EmailRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldFail()
    {
        var result = User.Create(
            "somesh@example.com",
            "",
            "Somesh",
            "Verma");

        Assert.True(result.IsFailure);
        Assert.Equal(
            UserErrors.PasswordHashRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldFail()
    {
        var result = User.Create(
            "somesh@example.com",
            "hashed-password",
            "   ",
            "Verma");

        Assert.True(result.IsFailure);
        Assert.Equal(
            UserErrors.FirstNameRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldFail()
    {
        var result = User.Create(
            "somesh@example.com",
            "hashed-password",
            "Somesh",
            "   ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            UserErrors.LastNameRequired,
            result.Error);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateProfile()
    {
        var user = CreateUser();

        var result = user.UpdateProfile(
            "  Updated  ",
            "  User  ",
            "  https://example.com/avatar.png  ");

        Assert.True(result.IsSuccess);

        Assert.Equal("Updated", user.FirstName);
        Assert.Equal("User", user.LastName);

        Assert.Equal(
            "https://example.com/avatar.png",
            user.AvatarUrl);
    }

    [Fact]
    public void UpdateProfile_WithWhitespaceAvatar_ShouldSetAvatarToNull()
    {
        var user = CreateUser();

        var result = user.UpdateProfile(
            "Somesh",
            "Verma",
            "   ");

        Assert.True(result.IsSuccess);
        Assert.Null(user.AvatarUrl);
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldChangePassword()
    {
        var user = CreateUser();

        var result = user.ChangePassword(
            "  new-password-hash  ");

        Assert.True(result.IsSuccess);

        Assert.Equal(
            "new-password-hash",
            user.PasswordHash);
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_ShouldFail()
    {
        var user = CreateUser();

        var originalPasswordHash =
            user.PasswordHash;

        var result = user.ChangePassword("   ");

        Assert.True(result.IsFailure);

        Assert.Equal(
            UserErrors.PasswordHashRequired,
            result.Error);

        Assert.Equal(
            originalPasswordHash,
            user.PasswordHash);
    }

    [Fact]
    public void ConfirmEmail_UnconfirmedUser_ShouldConfirmEmail()
    {
        var user = CreateUser();

        var result = user.ConfirmEmail();

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public void ConfirmEmail_AlreadyConfirmedUser_ShouldFail()
    {
        var user = CreateUser();

        var firstResult = user.ConfirmEmail();

        Assert.True(firstResult.IsSuccess);

        var secondResult = user.ConfirmEmail();

        Assert.True(secondResult.IsFailure);

        Assert.Equal(
            UserErrors.EmailAlreadyConfirmed,
            secondResult.Error);

        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetProvidedTimestamp()
    {
        var user = CreateUser();

        var utcNow = DateTime.UtcNow;

        user.UpdateLastLogin(utcNow);

        Assert.Equal(
            utcNow,
            user.LastLoginAtUtc);
    }

    private static User CreateUser()
    {
        var result = User.Create(
            "somesh@example.com",
            "hashed-password",
            "Somesh",
            "Verma");

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
