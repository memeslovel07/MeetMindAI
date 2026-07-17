using MediatR;

using MeetMindAI.Application.Common.Abstractions.AI;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Options;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.Extensions.Options;

namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.GenerateSummary;

public sealed class GenerateSummaryCommandHandler
    : IRequestHandler<GenerateSummaryCommand, Result<GenerateSummaryResponse>>
{
    private const string PromptVersion = "v1";

    private readonly IMeetingRepository _meetingRepository;
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IMeetingSummaryRepository _meetingSummaryRepository;
    private readonly IAiSummaryService _aiSummaryService;
    private readonly IApplicationDbContext _dbContext;
    private readonly AiOptions _aiOptions;


    public GenerateSummaryCommandHandler(
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

    public async Task<Result<GenerateSummaryResponse>> Handle(
        GenerateSummaryCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<GenerateSummaryResponse>.Failure(
                MeetingErrors.NotFound);
        }

        var transcript = await _transcriptRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (transcript is null)
        {
            return Result<GenerateSummaryResponse>.Failure(
                TranscriptErrors.NotFound);
        }

        var existingSummary = await _meetingSummaryRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (existingSummary is not null)
        {
            return Result<GenerateSummaryResponse>.Failure(
                MeetingSummaryErrors.AlreadyExists);
        }

        var aiResult = await _aiSummaryService.GenerateSummaryAsync(
            transcript.Content,
            cancellationToken);

        var summaryResult = MeetingSummary.Create(
      request.MeetingId,
      aiResult.Summary,
      aiResult.Provider,
      aiResult.Model,
      _aiOptions.PromptVersion);

        if (summaryResult.IsFailure)
        {
            return Result<GenerateSummaryResponse>.Failure(
                summaryResult.Error);
        }

        await _meetingSummaryRepository.AddAsync(
            summaryResult.Value,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<GenerateSummaryResponse>.Success(
            new GenerateSummaryResponse(
                summaryResult.Value.Id,
                summaryResult.Value.Summary));
    }
}
