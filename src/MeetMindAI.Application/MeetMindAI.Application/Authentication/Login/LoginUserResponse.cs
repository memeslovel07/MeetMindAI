using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Authentication.Login;

/// <summary>
/// Represents the response returned after a successful login.
/// </summary>
public sealed record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc);
