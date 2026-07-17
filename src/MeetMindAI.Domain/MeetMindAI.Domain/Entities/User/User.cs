<<<<<<< HEAD
using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Constants;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Entities.Users;

/// <summary>
/// Represents a registered user of the MeetMind AI platform.
/// </summary>
public sealed class User : AggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// Required by Entity Framework Core.
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="role">The user's role.</param>
    private User(
        string email,
        string normalizedEmail,
        string passwordHash,
        string firstName,
        string lastName)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = UserRole.User;

        EmailConfirmed = false;
        Status = EntityStatus.Active;
    }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the normalized email address.
    /// </summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the hashed password.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user's email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Gets the user's role.
    /// </summary>
    public UserRole Role { get; private set; } = UserRole.User;

    /// <summary>
    /// Gets the current status of the user.
    /// </summary>
    public EntityStatus Status { get; private set; } = EntityStatus.Active;

    /// <summary>
    /// Gets the UTC timestamp of the user's last successful login.
    /// </summary>
    public DateTime? LastLoginAtUtc { get; private set; }



    /// <summary>
    /// Creates a new <see cref="User"/> instance.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <returns>
    /// A successful result containing the created user, or a failure result if validation fails.
    /// </returns>
    public static Result<User> Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        email = email.Trim();

        var normalizedEmail = email.ToUpperInvariant();
        firstName = firstName.Trim();
        lastName = lastName.Trim();

        var validationResult = Validate(
            email,
            normalizedEmail,
            passwordHash,
            firstName,
            lastName);

        if (validationResult.IsFailure)
        {
            return Result<User>.Failure(validationResult.Error);
        }

        var user = new User(
            email,
            normalizedEmail,
            passwordHash,
            firstName,
            lastName);

        return Result<User>.Success(user);
    }


    private static Result Validate(
    string email,
    string normalizedEmail,
    string passwordHash,
    string firstName,
    string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(UserErrors.EmailRequired);
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result.Failure(UserErrors.NormalizedEmailRequired);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure(UserErrors.PasswordHashRequired);
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(UserErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(UserErrors.LastNameRequired);
        }

        if (email.Length > ValidationConstants.EmailMaxLength)
        {
            return Result.Failure(UserErrors.EmailTooLong);
        }

        if (passwordHash.Length > ValidationConstants.PasswordHashMaxLength)
        {
            return Result.Failure(UserErrors.PasswordHashTooLong);
        }

        if (firstName.Length > ValidationConstants.FirstNameMaxLength)
        {
            return Result.Failure(UserErrors.FirstNameTooLong);
        }

        if (lastName.Length > ValidationConstants.LastNameMaxLength)
        {
            return Result.Failure(UserErrors.LastNameTooLong);
        }

        if (normalizedEmail.Length > ValidationConstants.EmailMaxLength)
        {
            return Result.Failure(UserErrors.NormalizedEmailTooLong);
        }

        return Result.Success();

    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="firstName">The new first name.</param>
    /// <param name="lastName">The new last name.</param>
    /// <param name="avatarUrl">The new avatar URL.</param>
    /// <returns>
    /// A successful result if the profile was updated; otherwise, a failure result.
    /// </returns>
    public Result UpdateProfile(
        string firstName,
        string lastName,
        string? avatarUrl)
    {
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        avatarUrl = string.IsNullOrWhiteSpace(avatarUrl)
            ? null
            : avatarUrl.Trim();

        var validationResult = ValidateProfile(
            firstName,
            lastName,
            avatarUrl);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        FirstName = firstName;
        LastName = lastName;
        AvatarUrl = avatarUrl;

        return Result.Success();
    }

    private static Result ValidateProfile(
    string firstName,
    string lastName,
    string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(UserErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(UserErrors.LastNameRequired);
        }

        if (firstName.Length > ValidationConstants.FirstNameMaxLength)
        {
            return Result.Failure(UserErrors.FirstNameTooLong);
        }

        if (lastName.Length > ValidationConstants.LastNameMaxLength)
        {
            return Result.Failure(UserErrors.LastNameTooLong);
        }

        if (!string.IsNullOrWhiteSpace(avatarUrl) &&
            avatarUrl.Length > ValidationConstants.AvatarUrlMaxLength)
        {
            return Result.Failure(UserErrors.AvatarUrlTooLong);
        }

        return Result.Success();
    }

    public Result ChangePassword(string passwordHash)
    {
        passwordHash = passwordHash.Trim();

        var validation = ValidatePasswordHash(passwordHash);

        if (validation.IsFailure)
        {
            return validation;
        }

        PasswordHash = passwordHash;

        return Result.Success();
    }

    private static Result ValidatePasswordHash(
    string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure(
                UserErrors.PasswordHashRequired);
        }

        if (passwordHash.Length >
            ValidationConstants.PasswordHashMaxLength)
        {
            return Result.Failure(
                UserErrors.PasswordHashTooLong);
        }

        return Result.Success();
    }

    /// <summary>
    /// Confirms the user's email address.
    /// </summary>
    public Result ConfirmEmail()
    {
        if (EmailConfirmed)
        {
            return Result.Failure(UserErrors.EmailAlreadyConfirmed);
        }

        EmailConfirmed = true;

        return Result.Success();
    }

    public void UpdateLastLogin(DateTime utcNow)
    {
        LastLoginAtUtc = utcNow;
    }

}

=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Entities.User;
internal class User
{
}
>>>>>>> ae56db5 (shared on process)
