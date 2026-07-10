using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Infrastructure.Authentication;
using MeetMindAI.Infrastructure.Services;
using MeetMindAI.Persistence.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.Infrastructure;

/// <summary>
/// Registers infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services
     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {

         options.MapInboundClaims = false;

         var jwtOptions = configuration
             .GetSection(JwtOptions.SectionName)
             .Get<JwtOptions>()
             ?? throw new InvalidOperationException(
                 "JWT configuration was not found.");

         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,

             ValidIssuer = jwtOptions.Issuer,
             ValidAudience = jwtOptions.Audience,

             IssuerSigningKey = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

             ClockSkew = TimeSpan.Zero
         };
     });

        services.AddSingleton<
            Microsoft.Extensions.Options.IValidateOptions<JwtOptions>,
            JwtOptionsValidator>();

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
