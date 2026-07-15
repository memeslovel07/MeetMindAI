using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Common.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for the Meeting aggregate.
/// </summary>
public interface IMeetingRepository : IRepository<Meeting>
{
    /// <summary>
    /// Gets a meeting by its identifier.
    /// </summary>
    Task<Meeting?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all meetings organized by a user.
    /// </summary>
    Task<IReadOnlyList<Meeting>> GetByOrganizerIdAsync(
        Guid organizerId,
        CancellationToken cancellationToken = default);
}
