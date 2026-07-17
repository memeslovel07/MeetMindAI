using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Users.GetCurrentUser;

/// <summary>
/// Gets the currently authenticated user's profile.
/// </summary>
public sealed record GetCurrentUserQuery()
    : IRequest<Result<GetCurrentUserResponse>>;
