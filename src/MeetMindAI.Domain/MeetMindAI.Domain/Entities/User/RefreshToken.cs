using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Domain.Common;

namespace MeetMindAI.Domain.Entities.Users;

/// <summary>
/// Represents a refresh token issued to a user.
/// </summary>
public sealed class RefreshToken : AuditableEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// Required by Entity Framework Core.
    /// </summary>
    private RefreshToken()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// </summary>
    private RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAtUtc)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Gets the identifier of the user who owns the refresh token.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the refresh token value.
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the UTC date and time when the refresh token expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when the refresh token was revoked.
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets the replacement token when token rotation occurs.
    /// </summary>
    public Guid? ReplacedByRefreshTokenId { get; private set; }

    /// <summary>
    /// Gets the reason the token was revoked.
    /// </summary>
    public enum RefreshTokenRevocationReason
    {
        UserLogout,
        TokenRotation,
        PasswordChanged,
        SecurityBreach
    }

    /// <summary>
    /// Gets the user who owns the refresh token.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the refresh token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAtUtc.HasValue;

    /// <summary>
    /// Determines whether the refresh token has expired.
    /// </summary>
    public bool IsExpired(DateTime utcNow)
    {
        return utcNow >= ExpiresAtUtc;
    }

    /// <summary>
    /// Determines whether the refresh token is active.
    /// </summary>
    public bool IsActive(DateTime utcNow)
    {
        return !IsRevoked &&
               !IsExpired(utcNow);
    }

}
