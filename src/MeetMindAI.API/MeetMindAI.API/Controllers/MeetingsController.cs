using MediatR;
using MeetMindAI.Shared.Results;
using MeetMindAI.API.Contracts.Meetings;
using MeetMindAI.Application.Meetings.CreateMeeting;
using MeetMindAI.Application.Meetings.GetMeeting;
using MeetMindAI.Application.Meetings.GetMyMeetings;
using MeetMindAI.Application.Meetings.UpdateMeeting;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Application.Meetings.DeleteMeeting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMindAI.API.Controllers;

/// <summary>
/// Provides meeting management endpoints.
/// </summary>
[ApiController]
[Route("api/meetings")]
[Authorize]
public sealed class MeetingsController : ControllerBase
{
    private readonly ISender _sender;

    public MeetingsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Creates a new meeting.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(CreateMeetingResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateMeetingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new CreateMeetingCommand(
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Created(
            $"/api/meetings/{result.Value.MeetingId}",
            result.Value);
    }

    /// <summary>
    /// Gets a meeting by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(GetMeetingResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMeetingQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all meetings owned by the current user.
    /// </summary>
    [HttpGet("mine")]
    [ProducesResponseType(
        typeof(IReadOnlyList<GetMyMeetingsResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMyMeetingsQuery(),
            cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing meeting.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(UpdateMeetingResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateMeetingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new UpdateMeetingCommand(
            id,
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Forbidden)
            {
                return Forbid();
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
    /// Deletes a meeting.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        typeof(DeleteMeetingResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DeleteMeetingCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Forbidden)
            {
                return Forbid();
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
