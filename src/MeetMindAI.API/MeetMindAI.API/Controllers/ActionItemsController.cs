using MediatR;

using MeetMindAI.API.Contracts.ActionItems;
using MeetMindAI.Application.Features.ActionItems.Commands.ExtractActionItems;
using MeetMindAI.Application.Features.ActionItems.CompleteActionItem;
using MeetMindAI.Application.Features.ActionItems.CreateActionItem;
using MeetMindAI.Application.Features.ActionItems.DeleteActionItem;
using MeetMindAI.Application.Features.ActionItems.GetActionItem;
using MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;
using MeetMindAI.Application.Features.ActionItems.UpdateActionItem;
using MeetMindAI.Domain.Errors;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMindAI.API.Controllers;

/// <summary>
/// Provides action item management endpoints.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public sealed class ActionItemsController : ControllerBase
{
    private readonly ISender _sender;

    public ActionItemsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    [HttpPost("meetings/{meetingId:guid}/action-items")]
    [ProducesResponseType(
        typeof(CreateActionItemResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        Guid meetingId,
        CreateActionItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new CreateActionItemCommand(
            meetingId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Created(
            $"/api/action-items/{result.Value.Id}",
            result.Value);
    }

    [HttpGet("meetings/{meetingId:guid}/action-items")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ActionItemResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByMeeting(
        Guid meetingId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMeetingActionItemsQuery(meetingId),
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

    [HttpGet("action-items/{id:guid}")]
    [ProducesResponseType(
        typeof(GetActionItemResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetActionItemQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == ActionItemErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPut("action-items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateActionItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new UpdateActionItemCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == ActionItemErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPatch("action-items/{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CompleteActionItemCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == ActionItemErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("action-items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteActionItemCommand(
            id,
            null);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == ActionItemErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("extract")]
    public async Task<IActionResult> Extract(
    Guid meetingId,
    CancellationToken cancellationToken)
    {
        var command = new ExtractActionItemsCommand(
            meetingId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

}
