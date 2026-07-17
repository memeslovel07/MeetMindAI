namespace MeetMindAI.API.Contracts.Authentication;



/// <summary>
/// Request to log out the current user.
/// </summary>
public sealed record LogoutRequest(
    string RefreshToken);
