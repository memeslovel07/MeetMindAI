using MediatR;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.AI;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.Commands.ExtractActionItems;

public sealed class ExtractActionItemsCommandHandler
    : IRequestHandler<
        ExtractActionItemsCommand,
        Result<IReadOnlyList<Guid>>>
{
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IActionItemExtractionService _extractionService;
    private readonly IApplicationDbContext _dbContext;

    public ExtractActionItemsCommandHandler(
        ITranscriptRepository transcriptRepository,
        IActionItemRepository actionItemRepository,
        IActionItemExtractionService extractionService,
        IApplicationDbContext dbContext)
    {
        _transcriptRepository = transcriptRepository;
        _actionItemRepository = actionItemRepository;
        _extractionService = extractionService;
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        ExtractActionItemsCommand request,
        CancellationToken cancellationToken)
    {
        var transcript =
            await _transcriptRepository.GetByMeetingIdAsync(
                request.MeetingId,
                cancellationToken);

        if (transcript is null)
        {
            return Result<IReadOnlyList<Guid>>.Failure(
                new Error(
                    "Transcript.NotFound",
                    "Transcript was not found for the specified meeting."));
        }

        var existingActionItems =
    await _actionItemRepository.GetByMeetingIdAsync(
        request.MeetingId,
        cancellationToken);

        var hasAiGeneratedItems =
            existingActionItems.Any(
                item => item.Source == ActionItemSource.AiGenerated);

        if (hasAiGeneratedItems)
        {
            return Result<IReadOnlyList<Guid>>.Failure(
                new Error(
                    "ActionItems.AlreadyExtracted",
                    "AI action items have already been extracted for this meeting."));
        }

        var extractedItems =
            await _extractionService.ExtractAsync(
                transcript.Content,
                cancellationToken);

        if (extractedItems.Count == 0)
        {
            return Result<IReadOnlyList<Guid>>.Success(
                Array.Empty<Guid>());
        }

        var actionItemIds = new List<Guid>();

        foreach (var extractedItem in extractedItems)
        {
            var result = ActionItem.Create(
      request.MeetingId,
      extractedItem.Title,
      extractedItem.Description,
      extractedItem.Priority,
      extractedItem.DueDate,
      ActionItemSource.AiGenerated);

            if (result.IsFailure)
            {
                continue;
            }

            var actionItem = result.Value;

            await _actionItemRepository.AddAsync(
                actionItem,
                cancellationToken);

            actionItemIds.Add(actionItem.Id);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<IReadOnlyList<Guid>>.Success(
            actionItemIds);
    }
}
