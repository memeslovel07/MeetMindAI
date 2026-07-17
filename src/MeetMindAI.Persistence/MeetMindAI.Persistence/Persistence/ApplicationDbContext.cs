using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Persistence.Persistence;

/// <summary>
/// Represents the application's Entity Framework Core database context.
/// </summary>
public sealed class ApplicationDbContext
    : DbContext, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the users.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the refresh tokens.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Meeting> Meetings => Set<Meeting>();

    public DbSet<Transcript> Transcripts => Set<Transcript>();

    public DbSet<MeetingAttachment> MeetingAttachments => Set<MeetingAttachment>();

    public DbSet<MeetingSummary> MeetingSummaries => Set<MeetingSummary>();

    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
