using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Authentication.RefreshToken;

/// <summary>
/// Response returned after a successful refresh.
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc);
