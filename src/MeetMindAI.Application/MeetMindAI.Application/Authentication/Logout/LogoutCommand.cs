using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Authentication.Logout;

/// <summary>
/// Logs out the current user by revoking a refresh token.
/// </summary>
public sealed record LogoutCommand(
    string RefreshToken)
    : IRequest<Result>;
