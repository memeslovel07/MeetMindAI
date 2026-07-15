using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Common.Authorization;

/// <summary>
/// Authorization policy names.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);

    public const string UserOnly = nameof(UserOnly);
}
