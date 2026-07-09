namespace MeetMindAI.API.Contracts.Authentication;

/// <summary>
/// Represents a request to register a new user.
/// </summary>
public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
