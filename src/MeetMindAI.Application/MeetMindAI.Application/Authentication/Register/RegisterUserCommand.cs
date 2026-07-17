using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;


using MeetMindAI.Shared.Results; // Replace with your actual namespace

namespace MeetMindAI.Application.Authentication.Register;

/// <summary>
/// Represents a command to register a new user.
/// </summary>
public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password)
    : IRequest<Result<RegisterUserResponse>>;
