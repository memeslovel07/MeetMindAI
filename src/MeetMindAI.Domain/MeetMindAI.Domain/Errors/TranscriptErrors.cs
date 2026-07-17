using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

/// <summary>
/// Provides domain errors related to transcripts.
/// </summary>
public static class TranscriptErrors
{
    public static readonly Error MeetingRequired =
        new(
            "Transcript.MeetingRequired",
            "Meeting is required.");

    public static readonly Error ContentRequired =
        new(
            "Transcript.ContentRequired",
            "Transcript content is required.");

    public static readonly Error ContentTooLong =
        new(
            "Transcript.ContentTooLong",
            "Transcript content exceeds the maximum allowed length.");

    public static readonly Error InvalidDuration =
        new(
            "Transcript.InvalidDuration",
            "Transcript duration cannot be negative.");

    public static readonly Error AlreadyExists =
        new(
            "Transcript.AlreadyExists",
            "A transcript already exists for this meeting.");

    public static readonly Error NotFound =
        new(
            "Transcript.NotFound",
            "The requested transcript was not found.");

    public static readonly Error InvalidMeetingId =
    new(
        "Meeting.InvalidMeetingId",
        "The provided meeting ID is invalid.");

    public static readonly Error EmptyContent =
        new(
            "Transcript.EmptyContent",
            "Transcript content cannot be empty.");


    public static readonly Error InvalidMeeting =
        new(
            "Transcript.InvalidMeeting",
            "A valid meeting is required.");

  

    
}
