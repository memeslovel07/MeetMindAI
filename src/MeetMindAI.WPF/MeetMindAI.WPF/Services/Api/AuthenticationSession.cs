namespace MeetMindAI.WPF.Services.Authentication;

public sealed class AuthenticationSession
    : IAuthenticationSession
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public DateTime? AccessTokenExpiresAtUtc { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(AccessToken) &&
        AccessTokenExpiresAtUtc.HasValue &&
        AccessTokenExpiresAtUtc.Value > DateTime.UtcNow;

    public void SetTokens(
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        AccessTokenExpiresAtUtc = null;
    }
}
