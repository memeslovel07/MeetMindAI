using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Options;

using Microsoft.Extensions.Options;
using MeetMindAI.Application.Authentication;

namespace MeetMindAI.Infrastructure.Authentication;

/// <summary>
/// Generates JWT access tokens and refresh tokens.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    public JwtTokenGenerator(
        IOptions<JwtOptions> options,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public AuthenticationTokens Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = _dateTimeProvider.UtcNow;

        var accessTokenExpiresAtUtc = now.AddMinutes(
            _options.AccessTokenExpirationMinutes);

        var refreshTokenExpiresAtUtc = now.AddDays(
            _options.RefreshTokenExpirationDays);

        var accessToken = CreateAccessToken(
    user,
    accessTokenExpiresAtUtc);

        var refreshToken = GenerateRefreshToken();

        return new AuthenticationTokens(
            AccessToken: accessToken,
            AccessTokenExpiresAtUtc: accessTokenExpiresAtUtc,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAtUtc: refreshTokenExpiresAtUtc);
    }

    private string CreateAccessToken(
       User user,
       DateTime expiresAtUtc)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(JwtRegisteredClaimNames.GivenName, user.FirstName),
        new(JwtRegisteredClaimNames.FamilyName, user.LastName),
        new(ClaimTypes.Role, user.Role.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
    private static string GenerateRefreshToken()
    {
        Span<byte> bytes = stackalloc byte[64];

        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }
}
