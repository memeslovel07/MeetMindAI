using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Common.Abstractions.Services;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the authenticated user's identifier.
    /// Returns null when no user is authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
