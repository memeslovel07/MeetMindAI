using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

/// <summary>
/// Provides domain errors related to meeting summaries.
/// </summary>
public static class MeetingSummaryErrors
{
    public static readonly Error InvalidMeetingId =
        new(
            "MeetingSummary.InvalidMeetingId",
            "Meeting id is required.");

    public static readonly Error EmptySummary =
        new(
            "MeetingSummary.SummaryRequired",
            "Meeting summary is required.");

    public static readonly Error InvalidProvider =
        new(
            "MeetingSummary.ProviderRequired",
            "Provider is required.");

    public static readonly Error InvalidModel =
        new(
            "MeetingSummary.ModelRequired",
            "Model is required.");

    public static readonly Error InvalidPromptVersion =
        new(
            "MeetingSummary.PromptVersionRequired",
            "Prompt version is required.");

    public static readonly Error AlreadyExists =
        new(
            "MeetingSummary.AlreadyExists",
            "A summary already exists for this meeting.");

    public static readonly Error NotFound =
        new(
            "MeetingSummary.NotFound",
            "Meeting summary not found.");
}
