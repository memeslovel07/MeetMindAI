using MeetMindAI.WPF.Models.Transcripts;

namespace MeetMindAI.WPF.Services.Transcripts;

public interface ITranscriptApiService
{
    Task<TranscriptDetails?> GetAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    Task<CreateTranscriptResponse> CreateAsync(
        Guid meetingId,
        CreateTranscriptRequest request,
        CancellationToken cancellationToken = default);

    Task<UpdateTranscriptResponse> UpdateAsync(
        Guid meetingId,
        UpdateTranscriptRequest request,
        CancellationToken cancellationToken = default);
}
