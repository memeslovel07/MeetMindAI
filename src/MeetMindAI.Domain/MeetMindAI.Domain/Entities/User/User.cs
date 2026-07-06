using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Enums;

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
        string lastName,
        UserRole role)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = role;

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
}
