namespace MeetMindAI.API.Contracts.Authentication;

/// <summary>
/// Represents a login request.
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);
