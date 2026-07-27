using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.CompleteActionItem;

/// <summary>
/// Handles <see cref="CompleteActionItemCommand"/> requests.
/// </summary>
public sealed class CompleteActionItemCommandHandler
    : IRequestHandler<CompleteActionItemCommand, Result>
{
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IApplicationDbContext _dbContext;

    public CompleteActionItemCommandHandler(
        IActionItemRepository actionItemRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(actionItemRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _actionItemRepository = actionItemRepository;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        CompleteActionItemCommand request,
        CancellationToken cancellationToken)
    {
        var actionItem =
            await _actionItemRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (actionItem is null)
        {
            return Result.Failure(
                ActionItemErrors.NotFound);
        }

        var completeResult =
            actionItem.MarkCompleted();

        if (completeResult.IsFailure)
        {
            return completeResult;
        }

        _actionItemRepository.Update(actionItem);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
