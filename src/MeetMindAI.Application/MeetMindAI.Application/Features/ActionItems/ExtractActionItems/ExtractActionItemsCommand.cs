using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.Commands.ExtractActionItems;

public sealed record ExtractActionItemsCommand(
    Guid MeetingId)
    : IRequest<Result<IReadOnlyList<Guid>>>;
