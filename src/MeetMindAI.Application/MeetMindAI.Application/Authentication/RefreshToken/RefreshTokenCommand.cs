using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : IRequest<Result<RefreshTokenResponse>>;
