using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using MeetMindAI.Application.Common.Abstractions.Services;

using IdentityPasswordVerificationResult =
    Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace MeetMindAI.Infrastructure.Authentication;

/// <summary>
/// Provides password hashing using ASP.NET Core Identity.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(
            user: null!,
            password);
    }

    /// <inheritdoc />
    public Application.Authentication.PasswordVerificationResult Verify(
      string password,
      string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var result = _passwordHasher.VerifyHashedPassword(
            user: null!,
            hashedPassword: passwordHash,
            providedPassword: password);

        return result switch
        {
            IdentityPasswordVerificationResult.Success =>
                new MeetMindAI.Application.Authentication.PasswordVerificationResult(
                    true,
                    false),

            IdentityPasswordVerificationResult.SuccessRehashNeeded =>
                new MeetMindAI.Application.Authentication.PasswordVerificationResult(
                    true,
                    true),

            _ =>
                new MeetMindAI.Application.Authentication.PasswordVerificationResult(
                    false,
                    false)
        };
    }
}
