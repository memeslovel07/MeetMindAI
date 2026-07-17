using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Application.Authentication;
using MeetMindAI.Domain.Entities.Users;

namespace MeetMindAI.Application.Common.Abstractions.Services;

/// <summary>
/// Generates JSON Web Tokens (JWT) for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates authentication tokens for the specified user.
    /// </summary>
    /// <param name="user">
    /// The authenticated user.
    /// </param>
    /// <returns>
    /// The generated authentication tokens.
    /// </returns>
    AuthenticationTokens Generate(User user);
}
