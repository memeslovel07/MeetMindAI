using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;
using MeetMindAI.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.Persistence;

/// <summary>
/// Registers persistence services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence services.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "The database connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IMeetingRepository, MeetingRepository>();

        services.AddScoped<ITranscriptRepository, TranscriptRepository>();

        services.AddScoped<IMeetingAttachmentRepository, MeetingAttachmentRepository>();

        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<ITranscriptRepository, TranscriptRepository>();
        

        services.AddScoped<IMeetingSummaryRepository, MeetingSummaryRepository>();

        return services;
    }
}
