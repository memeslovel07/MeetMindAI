using MeetMindAI.WPF.Models.MeetingAttachments;

namespace MeetMindAI.WPF.Services.Dialogs;

public interface IAttachmentDialogService
{
    string? SelectFile();

    Task SaveAndOpenAsync(
        DownloadedAttachment attachment,
        CancellationToken cancellationToken = default);

    bool ConfirmDelete(
        MeetingAttachmentItem attachment);
}
