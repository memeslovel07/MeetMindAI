using MediatR;

using MeetMindAI.API.Contracts.Transcripts;
using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Application.Features.Transcripts.DeleteTranscript;
using MeetMindAI.Application.Features.Transcripts.GetTranscript;
using MeetMindAI.Application.Features.Transcripts.UpdateTranscript;
using MeetMindAI.Application.Transcripts.DeleteTranscript;
using MeetMindAI.Application.Transcripts.UpdateTranscript;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMindAI.API.Controllers;

/// <summary>
/// Provides transcript management endpoints.
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId:guid}/transcript")]
[Authorize]
public sealed class TranscriptsController : ControllerBase
{
    private readonly ISender _sender;

    public TranscriptsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Creates a transcript for a meeting.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(CreateTranscriptResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        Guid meetingId,
        CreateTranscriptRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var duration = request.DurationSeconds.HasValue
    ? TimeSpan.FromSeconds(request.DurationSeconds.Value)
    : (TimeSpan?)null;

        var command = new CreateTranscriptCommand(
            meetingId,
            request.Content,
            request.Language,
            duration);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == TranscriptErrors.AlreadyExists)
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
            $"/api/meetings/{meetingId}/transcript",
            result.Value);
    }

    

[HttpGet]
[ProducesResponseType(
    typeof(GetTranscriptResponse),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Get(
    Guid meetingId,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new GetTranscriptQuery(meetingId),
        cancellationToken);

    if (result.IsFailure)
    {
        if (result.Error == TranscriptErrors.NotFound)
        {
            return NotFound(result.Error);
        }

        return BadRequest(result.Error);
    }

    return Ok(result.Value);
}

   

[HttpPut]
[ProducesResponseType(
    typeof(UpdateTranscriptResponse),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Update(
    Guid meetingId,
    UpdateTranscriptRequest request,
    CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(request);

        var duration = request.DurationSeconds.HasValue
            ? TimeSpan.FromSeconds(request.DurationSeconds.Value)
            : (TimeSpan?)null;

        var command = new UpdateTranscriptCommand(
      meetingId,
      request.Content,
      request.Language,
      duration);

        var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure)
    {
        if (result.Error == TranscriptErrors.NotFound)
        {
            return NotFound(result.Error);
        }

        return BadRequest(result.Error);
    }

    return Ok(result.Value);
}


[HttpDelete]
[ProducesResponseType(
    typeof(DeleteTranscriptResponse),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(
    Guid meetingId,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new DeleteTranscriptCommand(meetingId),
        cancellationToken);

    if (result.IsFailure)
    {
        if (result.Error == TranscriptErrors.NotFound)
        {
            return NotFound(result.Error);
        }

        return BadRequest(result.Error);
    }

    return Ok(result.Value);
}
}
