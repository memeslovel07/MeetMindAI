using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.DeleteActionItem;

public sealed class DeleteActionItemCommandHandler
    : IRequestHandler<DeleteActionItemCommand, Result>
{
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IApplicationDbContext _dbContext;

    public DeleteActionItemCommandHandler(
        IActionItemRepository actionItemRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(actionItemRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _actionItemRepository = actionItemRepository;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        DeleteActionItemCommand request,
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

        var deleteResult = actionItem.Delete(
            request.DeletedBy,
            DateTime.UtcNow);

        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        _actionItemRepository.Update(actionItem);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
