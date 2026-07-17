using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Persistence.Persistence;

using Microsoft.EntityFrameworkCore;

namespace MeetMindAI.Persistence.Repositories;

public sealed class MeetingAttachmentRepository : IMeetingAttachmentRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingAttachmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        MeetingAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        await _context.MeetingAttachments.AddAsync(attachment, cancellationToken);
    }

    public async Task<MeetingAttachment?> GetByIdAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingAttachments
            .FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<MeetingAttachment>> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingAttachments
            .Where(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken);
    }

    public void Remove(MeetingAttachment attachment)
    {
        _context.MeetingAttachments.Remove(attachment);
    }
}
