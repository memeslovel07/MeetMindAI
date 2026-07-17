<<<<<<< HEAD
using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Entities.Meetings;

public sealed class Transcript : AuditableEntity
{
    public const int MaxContentLength = 100_000;
    public const int MaxLanguageLength = 20;

    private Transcript()
    {
    }

    private Transcript(
        Guid meetingId,
        string content,
        string? language,
        TimeSpan? duration)
    {
        Id = Guid.NewGuid();

        MeetingId = meetingId;
        Content = content;
        Language = language;
        Duration = duration;
    }

    public Guid MeetingId { get; private set; }

    public Meeting Meeting { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;

    public string? Language { get; private set; }

    public TimeSpan? Duration { get; private set; }

    public static Result<Transcript> Create(
        Guid meetingId,
        string content,
        string? language,
        TimeSpan? duration)
    {
        if (meetingId == Guid.Empty)
        {
            return Result<Transcript>.Failure(
                TranscriptErrors.InvalidMeetingId);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<Transcript>.Failure(
                TranscriptErrors.EmptyContent);
        }

        content = content.Trim();

        if (content.Length > MaxContentLength)
        {
            return Result<Transcript>.Failure(
                TranscriptErrors.ContentTooLong);
        }

        if (duration.HasValue && duration.Value < TimeSpan.Zero)
        {
            return Result<Transcript>.Failure(
                TranscriptErrors.InvalidDuration);
        }

        return Result<Transcript>.Success(
            new Transcript(
                meetingId,
                content,
                language?.Trim(),
                duration));
    }

    public Result UpdateContent(
        string content,
        string? language,
        TimeSpan? duration)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure(
                TranscriptErrors.EmptyContent);
        }

        content = content.Trim();

        if (content.Length > MaxContentLength)
        {
            return Result.Failure(
                TranscriptErrors.ContentTooLong);
        }

        if (duration.HasValue && duration.Value < TimeSpan.Zero)
        {
            return Result.Failure(
                TranscriptErrors.InvalidDuration);
        }

        Content = content;
        Language = language?.Trim();
        Duration = duration;

        return Result.Success();
    }
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Entities.Meeting;
internal class Transcript
{
>>>>>>> ae56db5 (shared on process)
}
