using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Users.GetCurrentUser;

/// <summary>
/// Represents the current user's profile.
/// </summary>
public sealed record GetCurrentUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool EmailConfirmed,
    DateTime? LastLoginAtUtc);
