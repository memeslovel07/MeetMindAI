using Microsoft.AspNetCore.Http;

namespace MeetMindAI.API.Contracts.MeetingAttachments;

public sealed class UploadAttachmentRequest
{
    public IFormFile File { get; init; } = null!;
}
