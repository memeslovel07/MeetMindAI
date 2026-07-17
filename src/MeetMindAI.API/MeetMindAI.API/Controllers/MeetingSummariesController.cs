using MediatR;

using MeetMindAI.Application.Features.MeetingSummaries.Commands.GenerateSummary;
using MeetMindAI.Application.Features.MeetingSummaries.Commands.GetMeetingSummary;
using MeetMindAI.Application.Features.MeetingSummaries.Commands.RegenerateSummary;
using MeetMindAI.Application.Features.MeetingSummaries.Queries.GetMeetingSummary;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMindAI.API.Controllers;

/// <summary>
/// Provides AI meeting summary management endpoints.
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId:guid}/summary")]
[Authorize]
public sealed class MeetingSummariesController : ControllerBase
{
    private readonly ISender _sender;

    public MeetingSummariesController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Generates an AI summary for a meeting transcript.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(GenerateSummaryResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Generate(
        Guid meetingId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GenerateSummaryCommand(meetingId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == TranscriptErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == MeetingSummaryErrors.AlreadyExists)
            {
                return Conflict(result.Error);
            }

            if (result.Error == Error.Unauthorized)
            {
                return Unauthorized(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Created(
            $"/api/meetings/{meetingId}/summary",
            result.Value);
    }

    /// <summary>
    /// Gets the AI-generated summary for a meeting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(GetMeetingSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid meetingId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMeetingSummaryQuery(meetingId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingSummaryErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Unauthorized)
            {
                return Unauthorized(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Regenerates the AI summary for a meeting transcript.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(
        typeof(RegenerateSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Regenerate(
        Guid meetingId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RegenerateSummaryCommand(meetingId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == TranscriptErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == MeetingSummaryErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Unauthorized)
            {
                return Unauthorized(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
