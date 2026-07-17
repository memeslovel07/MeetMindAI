using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Application.Authentication;

namespace MeetMindAI.Application.Common.Abstractions.Services;

/// <summary>
/// Defines password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// </summary>
   public PasswordVerificationResult Verify(
        string password,
        string passwordHash);
}
