using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace MeetMindAI.Persistence.Options;

/// <summary>
/// Validates JWT configuration options.
/// </summary>
public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
            return ValidateOptionsResult.Fail("JWT issuer is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            return ValidateOptionsResult.Fail("JWT audience is required.");

        if (string.IsNullOrWhiteSpace(options.SecretKey))
            return ValidateOptionsResult.Fail("JWT secret key is required.");

        if (options.SecretKey.Length < 32)
            return ValidateOptionsResult.Fail(
                "JWT secret key must be at least 32 characters.");

        if (options.AccessTokenExpirationMinutes <= 0)
            return ValidateOptionsResult.Fail(
                "Access token expiration must be greater than zero.");

        if (options.RefreshTokenExpirationDays <= 0)
            return ValidateOptionsResult.Fail(
                "Refresh token expiration must be greater than zero.");

        return ValidateOptionsResult.Success;
    }
}
