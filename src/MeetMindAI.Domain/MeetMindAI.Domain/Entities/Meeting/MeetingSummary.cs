using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Entities.Meetings;

/// <summary>
/// Represents an AI-generated summary for a meeting.
/// </summary>
public sealed class MeetingSummary : AuditableEntity
{
    private MeetingSummary()
    {
    }

    private MeetingSummary(
        Guid meetingId,
        string summary,
        string provider,
        string model,
        string promptVersion)
    {
        Id = Guid.NewGuid();
        MeetingId = meetingId;
        Summary = summary;
        Provider = provider;
        Model = model;
        PromptVersion = promptVersion;
        GeneratedAtUtc = DateTime.UtcNow;
        IsRegenerated = false;
    }

    public Guid MeetingId { get; private set; }

    public Meeting Meeting { get; private set; } = null!;

    public string Summary { get; private set; } = string.Empty;

    public string Provider { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public string PromptVersion { get; private set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; private set; }

    public bool IsRegenerated { get; private set; }

    public static Result<MeetingSummary> Create(
        Guid meetingId,
        string summary,
        string provider,
        string model,
        string promptVersion)
    {
        if (meetingId == Guid.Empty)
        {
            return Result<MeetingSummary>.Failure(
                MeetingSummaryErrors.InvalidMeetingId);
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result<MeetingSummary>.Failure(
                MeetingSummaryErrors.EmptySummary);
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result<MeetingSummary>.Failure(
                MeetingSummaryErrors.InvalidProvider);
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return Result<MeetingSummary>.Failure(
                MeetingSummaryErrors.InvalidModel);
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            return Result<MeetingSummary>.Failure(
                MeetingSummaryErrors.InvalidPromptVersion);
        }

        var validationResult = Validate(
    summary,
    provider,
    model,
    promptVersion);

        if (validationResult.IsFailure)
        {
            return Result<MeetingSummary>.Failure(
                validationResult.Error);
        }

        return Result<MeetingSummary>.Success(
            new MeetingSummary(
                meetingId,
                summary.Trim(),
                provider.Trim(),
                model.Trim(),
                promptVersion.Trim()));
    }

    public Result Regenerate(
        string summary,
        string provider,
        string model,
        string promptVersion)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure(MeetingSummaryErrors.InvalidProvider);
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return Result.Failure(MeetingSummaryErrors.InvalidModel);
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            return Result.Failure(MeetingSummaryErrors.InvalidPromptVersion);
        }

     
        var validationResult = Validate(
    summary,
    provider,
    model,
    promptVersion);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Summary = summary.Trim();
        Provider = provider.Trim();
        Model = model.Trim();
        PromptVersion = promptVersion.Trim();
        GeneratedAtUtc = DateTime.UtcNow;
        IsRegenerated = true;

        return Result.Success();
    }

    private static Result Validate(
    string summary,
    string provider,
    string model,
    string promptVersion)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result.Failure(
                MeetingSummaryErrors.EmptySummary);
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure(
                MeetingSummaryErrors.InvalidProvider);
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return Result.Failure(
                MeetingSummaryErrors.InvalidModel);
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            return Result.Failure(
                MeetingSummaryErrors.InvalidPromptVersion);
        }

        return Result.Success();
    }
}
