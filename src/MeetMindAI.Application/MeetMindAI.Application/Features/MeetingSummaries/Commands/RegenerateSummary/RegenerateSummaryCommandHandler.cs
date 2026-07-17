using MediatR;

using MeetMindAI.Application.Common.Abstractions.AI;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Options;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.Extensions.Options;

namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.RegenerateSummary;

/// <summary>
/// Handles regeneration of an AI meeting summary.
/// </summary>
public sealed class RegenerateSummaryCommandHandler
    : IRequestHandler<RegenerateSummaryCommand, Result<RegenerateSummaryResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IMeetingSummaryRepository _meetingSummaryRepository;
    private readonly IAiSummaryService _aiSummaryService;
    private readonly IApplicationDbContext _dbContext;
    private readonly AiOptions _aiOptions;

    public RegenerateSummaryCommandHandler(
        IMeetingRepository meetingRepository,
        ITranscriptRepository transcriptRepository,
        IMeetingSummaryRepository meetingSummaryRepository,
        IAiSummaryService aiSummaryService,
        IApplicationDbContext dbContext,
        IOptions<AiOptions> aiOptions)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(transcriptRepository);
        ArgumentNullException.ThrowIfNull(meetingSummaryRepository);
        ArgumentNullException.ThrowIfNull(aiSummaryService);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(aiOptions);

        _meetingRepository = meetingRepository;
        _transcriptRepository = transcriptRepository;
        _meetingSummaryRepository = meetingSummaryRepository;
        _aiSummaryService = aiSummaryService;
        _dbContext = dbContext;
        _aiOptions = aiOptions.Value;
    }

    public async Task<Result<RegenerateSummaryResponse>> Handle(
        RegenerateSummaryCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<RegenerateSummaryResponse>.Failure(
                MeetingErrors.NotFound);
        }

        var transcript = await _transcriptRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (transcript is null)
        {
            return Result<RegenerateSummaryResponse>.Failure(
                TranscriptErrors.NotFound);
        }

        var summary = await _meetingSummaryRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (summary is null)
        {
            return Result<RegenerateSummaryResponse>.Failure(
                MeetingSummaryErrors.NotFound);
        }

        var aiResult = await _aiSummaryService.GenerateSummaryAsync(
            transcript.Content,
            cancellationToken);

        var regenerateResult = summary.Regenerate(
            aiResult.Summary,
            aiResult.Provider,
            aiResult.Model,
            _aiOptions.PromptVersion);

        if (regenerateResult.IsFailure)
        {
            return Result<RegenerateSummaryResponse>.Failure(
                regenerateResult.Error);
        }

        _meetingSummaryRepository.Update(summary);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<RegenerateSummaryResponse>.Success(
            new RegenerateSummaryResponse(
                summary.Id,
                summary.Summary));
    }
}
