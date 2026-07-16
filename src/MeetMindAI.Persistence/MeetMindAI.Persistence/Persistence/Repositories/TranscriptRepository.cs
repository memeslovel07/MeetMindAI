using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;

using Microsoft.EntityFrameworkCore;


namespace MeetMindAI.Persistence.Persistence.Repositories;



public sealed class TranscriptRepository : ITranscriptRepository
{
    private readonly ApplicationDbContext _context;

    public TranscriptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Transcript transcript,
        CancellationToken cancellationToken = default)
    {
        await _context.Transcripts.AddAsync(transcript, cancellationToken);
    }

    public async Task<Transcript?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Transcript?> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts
            .FirstOrDefaultAsync(x => x.MeetingId == meetingId, cancellationToken);
    }

    public void Update(Transcript transcript)
    {
        _context.Transcripts.Update(transcript);
    }

    public void Remove(Transcript transcript)
    {
        _context.Transcripts.Remove(transcript);
    }
}
