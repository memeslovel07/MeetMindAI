using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Application.Common.Abstractions.AI;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Authorization;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Infrastructure.AI.Mock;
using MeetMindAI.Infrastructure.Authentication;
using MeetMindAI.Infrastructure.Services;
using MeetMindAI.Infrastructure.Storage;
using MeetMindAI.Infrastructure.Storage.Local;
using MeetMindAI.Persistence.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy =>
                {
                    policy.RequireRole(UserRole.Admin.ToString());
                });

            options.AddPolicy(
                AuthorizationPolicies.UserOnly,
                policy =>
                {
                    policy.RequireRole(
                        UserRole.User.ToString(),
                        UserRole.Admin.ToString());
                });
        });

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

        services.Configure<LocalStorageOptions>(
    configuration.GetSection(LocalStorageOptions.SectionName));

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddScoped<IAiSummaryService, MockAiSummaryService>();



        return services;
    }
}
