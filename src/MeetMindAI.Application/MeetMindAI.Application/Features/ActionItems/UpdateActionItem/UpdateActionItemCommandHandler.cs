using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.UpdateActionItem;

/// <summary>
/// Handles <see cref="UpdateActionItemCommand"/> requests.
/// </summary>
public sealed class UpdateActionItemCommandHandler
    : IRequestHandler<UpdateActionItemCommand, Result>
{
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IApplicationDbContext _dbContext;

    public UpdateActionItemCommandHandler(
        IActionItemRepository actionItemRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(actionItemRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _actionItemRepository = actionItemRepository;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        UpdateActionItemCommand request,
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

        var updateResult = actionItem.Update(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _actionItemRepository.Update(actionItem);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
