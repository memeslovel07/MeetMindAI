using MediatR;

using MeetMindAI.API.Contracts.Meetings;
using MeetMindAI.Application.Meetings.CreateMeeting;

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
}
