using MediatR;

using MeetMindAI.API.Contracts.MeetingAttachments;
using MeetMindAI.Application.Features.MeetingAttachments.Common;
using MeetMindAI.Application.Features.MeetingAttachments.DeleteAttachment;
using MeetMindAI.Application.Features.MeetingAttachments.DownloadAttachment;
using MeetMindAI.Application.Features.MeetingAttachments.GetMeetingAttachments;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMindAI.API.Controllers;

/// <summary>
/// Provides meeting attachment management endpoints.
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId:guid}/attachments")]
[Authorize]
public sealed class MeetingAttachmentsController : ControllerBase
{
    private readonly ISender _sender;

    public MeetingAttachmentsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Uploads an attachment for a meeting.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(MeetingAttachmentResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        Guid meetingId,
        [FromForm] UploadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new UploadAttachmentCommand(
            meetingId,
            request.File);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Unauthorized)
            {
                return Unauthorized(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Created(
            $"/api/meetings/{meetingId}/attachments/{result.Value.Id}",
            result.Value);
    }

    /// <summary>
    /// Gets all attachments for a meeting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<GetMeetingAttachmentResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(
        Guid meetingId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMeetingAttachmentsQuery(meetingId),
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
    /// Downloads an attachment.
    /// </summary>
    [HttpGet("{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Download(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DownloadAttachmentQuery(
                meetingId,
                attachmentId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == MeetingAttachmentErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == MeetingAttachmentErrors.FileNotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == Error.Unauthorized)
            {
                return Unauthorized(result.Error);
            }

            return BadRequest(result.Error);
        }

        return File(
            result.Value.Stream,
            result.Value.ContentType,
            result.Value.FileName);
    }

    /// <summary>
    /// Deletes an attachment.
    /// </summary>
    [HttpDelete("{attachmentId:guid}")]
    [ProducesResponseType(
        typeof(DeleteAttachmentResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DeleteAttachmentCommand(
                meetingId,
                attachmentId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == MeetingErrors.NotFound)
            {
                return NotFound(result.Error);
            }

            if (result.Error == MeetingAttachmentErrors.NotFound)
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
