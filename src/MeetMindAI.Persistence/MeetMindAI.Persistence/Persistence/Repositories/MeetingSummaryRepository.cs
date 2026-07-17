using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Persistence.Persistence.Repositories;

public sealed class MeetingSummaryRepository
    : Repository<MeetingSummary>, IMeetingSummaryRepository
{
    public MeetingSummaryRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<MeetingSummary?> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        return await Context.MeetingSummaries
            .SingleOrDefaultAsync(
                x => x.MeetingId == meetingId,
                cancellationToken);
    }
}
