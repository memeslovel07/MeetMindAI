using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Authentication;

/// <summary>
/// Represents the result of verifying a password.
/// </summary>
/// <param name="Succeeded">
/// Indicates whether the password verification succeeded.
/// </param>
/// <param name="RequiresRehash">
/// Indicates whether the password hash should be upgraded.
/// </param>
public sealed record PasswordVerificationResult(
    bool Succeeded,
    bool RequiresRehash);
