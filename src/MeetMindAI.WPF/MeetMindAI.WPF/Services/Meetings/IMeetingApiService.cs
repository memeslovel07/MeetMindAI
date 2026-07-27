using MeetMindAI.WPF.Models.Meetings;

namespace MeetMindAI.WPF.Services.Meetings;

public interface IMeetingApiService
{
    Task<IReadOnlyList<MeetingListItem>> GetMineAsync(
        CancellationToken cancellationToken = default);

    Task<CreateMeetingResponse> CreateAsync(
        CreateMeetingRequest request,
        CancellationToken cancellationToken = default);

    Task<MeetingDetails> GetByIdAsync(
    Guid meetingId,
    CancellationToken cancellationToken = default);

}
