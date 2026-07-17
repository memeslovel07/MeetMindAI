using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Authentication.Login;

/// <summary>
/// Represents a request to authenticate a user.
/// </summary>
public sealed record LoginUserCommand(
    string Email,
    string Password)
    : IRequest<Result<LoginUserResponse>>;
