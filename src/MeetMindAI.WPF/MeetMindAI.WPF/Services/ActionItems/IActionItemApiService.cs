using MeetMindAI.WPF.Models.ActionItems;

namespace MeetMindAI.WPF.Services.ActionItems;

public interface IActionItemApiService
{
    Task<IReadOnlyList<ActionItemDetails>> GetByMeetingAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    Task<CreateActionItemResponse> CreateAsync(
        Guid meetingId,
        CreateActionItemRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateActionItemRequest request,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ExtractAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);
}
