using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Features.MeetingSummaries.Commands.GetMeetingSummary;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingSummaries.Queries.GetMeetingSummary;

public sealed class GetMeetingSummaryQueryHandler
    : IRequestHandler<GetMeetingSummaryQuery, Result<GetMeetingSummaryResponse>>
{
    private readonly IMeetingSummaryRepository _meetingSummaryRepository;

    public GetMeetingSummaryQueryHandler(
        IMeetingSummaryRepository meetingSummaryRepository)
    {
        ArgumentNullException.ThrowIfNull(meetingSummaryRepository);

        _meetingSummaryRepository = meetingSummaryRepository;
    }

    public async Task<Result<GetMeetingSummaryResponse>> Handle(
        GetMeetingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _meetingSummaryRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (summary is null)
        {
            return Result<GetMeetingSummaryResponse>.Failure(
                MeetingSummaryErrors.NotFound);
        }

        return Result<GetMeetingSummaryResponse>.Success(
            new GetMeetingSummaryResponse(
                summary.Id,
                summary.MeetingId,
                summary.Summary,
                summary.Provider,
                summary.Model,
                summary.PromptVersion,
                summary.GeneratedAtUtc,
                summary.IsRegenerated));
    }
}
