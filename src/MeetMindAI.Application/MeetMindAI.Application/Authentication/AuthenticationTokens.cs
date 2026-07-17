using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Authentication;

/// <summary>
/// Represents the authentication tokens issued after a successful authentication.
/// </summary>
public sealed record AuthenticationTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
