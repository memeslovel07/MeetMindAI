<<<<<<< HEAD
using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Enums;

namespace MeetMindAI.Domain.Entities.Users;

/// <summary>
/// Represents a refresh token issued to a user.
/// </summary>
public sealed class RefreshToken : AuditableEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class. Required by Entity
    /// Framework Core.
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
    /// Gets the user who owns the refresh token.
    /// </summary>
    public User User { get; private set; } = null!;

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
    /// Gets the refresh token that replaced this token during rotation.
    /// </summary>
    public RefreshToken? ReplacedByRefreshToken { get; private set; }


    /// <summary>
    /// Gets the reason why the refresh token was revoked.
    /// </summary>
    public RefreshTokenRevocationReason? RevocationReason { get; private set; }

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

    /// <summary>
    /// Creates a new refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The refresh token value.</param>
    /// <param name="expiresAtUtc">The UTC expiration date and time.</param>
    /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required arguments are invalid.</exception>
    public static RefreshToken Create(
    Guid userId,
    string token,
    DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));

        var now = DateTime.UtcNow;

        if (expiresAtUtc <= now)
            throw new ArgumentException(
                "Expiration date must be in the future.",
                nameof(expiresAtUtc));

        token = token.Trim();

        var refreshToken = new RefreshToken(
            userId,
            token,
            expiresAtUtc);

        refreshToken.CreatedAtUtc = now;

        return refreshToken;
    }

    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    /// <param name="reason">
    /// The reason why the refresh token is being revoked.
    /// </param>
    /// <param name="replacementToken">
    /// The replacement refresh token created during token rotation, if any.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the refresh token has already been revoked.
    /// </exception>
    public void Revoke(
        RefreshTokenRevocationReason reason,
        RefreshToken? replacementToken = null)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException(
                "The refresh token has already been revoked.");
        }

        RevokedAtUtc = DateTime.UtcNow;
        RevocationReason = reason;

        if (replacementToken is not null)
        {
            ReplacedByRefreshToken = replacementToken;
            ReplacedByRefreshTokenId = replacementToken.Id;
        }

        UpdatedAtUtc = RevokedAtUtc;
    }

    /// <summary>
    /// Rotates the refresh token by creating a replacement token and revoking the current token.
    /// </summary>
    /// <param name="token">The new refresh token value.</param>
    /// <param name="expiresAtUtc">The UTC expiration date of the replacement token.</param>
    /// <returns>The newly created refresh token.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current refresh token has already been revoked.
    /// </exception>
    public RefreshToken Rotate(
        string token,
        DateTime expiresAtUtc)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException(
                "A revoked refresh token cannot be rotated.");
        }

        var replacementToken = Create(
            UserId,
            token,
            expiresAtUtc);

        Revoke(
            RefreshTokenRevocationReason.TokenRotation,
            replacementToken);

        return replacementToken;
    }

=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Entities.User;
internal class RefreshToken
{
>>>>>>> ae56db5 (shared on process)
}
