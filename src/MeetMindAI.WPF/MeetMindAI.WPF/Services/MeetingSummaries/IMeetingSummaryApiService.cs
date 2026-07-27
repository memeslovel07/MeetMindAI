using MeetMindAI.WPF.Models.MeetingSummaries;

namespace MeetMindAI.WPF.Services.MeetingSummaries;

public interface IMeetingSummaryApiService
{
    Task<MeetingSummaryDetails?> GetAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    Task<GenerateSummaryResponse> GenerateAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    Task<RegenerateSummaryResponse> RegenerateAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);
}
