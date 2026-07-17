namespace MeetMindAI.Domain.Enums;

/// <summary>
/// Represents the reason why a refresh token was revoked.
/// </summary>
public enum RefreshTokenRevocationReason
{
    /// <summary>
    /// The user explicitly logged out.
    /// </summary>
    UserLogout = 1,

    /// <summary>
    /// The refresh token was replaced during token rotation.
    /// </summary>
    TokenRotation = 2,

    /// <summary>
    /// The user's password was changed.
    /// </summary>
    PasswordChanged = 3,

    /// <summary>
    /// The token was revoked due to suspected security compromise.
    /// </summary>
    SecurityBreach = 4
}
