namespace MeetMindAI.WPF.Services.Authentication;

public interface IAuthenticationSession
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    DateTime? AccessTokenExpiresAtUtc { get; }

    bool IsAuthenticated { get; }

    void SetTokens(
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAtUtc);

    void Clear();
}
