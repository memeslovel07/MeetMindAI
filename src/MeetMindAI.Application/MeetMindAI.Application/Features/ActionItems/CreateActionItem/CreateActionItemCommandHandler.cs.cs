using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.CreateActionItem;

/// <summary>
/// Handles <see cref="CreateActionItemCommand"/> requests.
/// </summary>
public sealed class CreateActionItemCommandHandler
    : IRequestHandler<
        CreateActionItemCommand,
        Result<CreateActionItemResponse>>
{
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly IApplicationDbContext _dbContext;

    public CreateActionItemCommandHandler(
        IActionItemRepository actionItemRepository,
        IMeetingRepository meetingRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(actionItemRepository);
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _actionItemRepository = actionItemRepository;
        _meetingRepository = meetingRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<CreateActionItemResponse>> Handle(
        CreateActionItemCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<CreateActionItemResponse>.Failure(
                MeetingErrors.NotFound);
        }

        var actionItemResult = ActionItem.Create(
            request.MeetingId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        if (actionItemResult.IsFailure)
        {
            return Result<CreateActionItemResponse>.Failure(
                actionItemResult.Error);
        }

        await _actionItemRepository.AddAsync(
            actionItemResult.Value,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CreateActionItemResponse>.Success(
            new CreateActionItemResponse(
                actionItemResult.Value.Id));
    }
}
