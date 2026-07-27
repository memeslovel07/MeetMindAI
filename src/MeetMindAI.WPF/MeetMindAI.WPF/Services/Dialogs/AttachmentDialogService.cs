using System.Diagnostics;
using System.IO;
using System.Windows;

using MeetMindAI.WPF.Models.MeetingAttachments;

using Microsoft.Win32;

namespace MeetMindAI.WPF.Services.Dialogs;

public sealed class AttachmentDialogService
    : IAttachmentDialogService
{
    public string? SelectFile()
    {
        var dialog =
            new OpenFileDialog
            {
                Title = "Select Meeting Attachment",
                CheckFileExists = true,
                Multiselect = false,
                Filter =
                    "Supported files|*.pdf;*.txt;*.doc;*.docx;*.xls;*.xlsx;*.png;*.jpg;*.jpeg;*.mp3;*.wav;*.mp4|" +
                    "All files|*.*"
            };

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }

    public async Task SaveAndOpenAsync(
        DownloadedAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var downloadsFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                "Downloads",
                "MeetMindAI");

        Directory.CreateDirectory(
            downloadsFolder);

        var safeFileName =
            Path.GetFileName(
                attachment.FileName);

        var filePath =
            Path.Combine(
                downloadsFolder,
                safeFileName);

        await File.WriteAllBytesAsync(
            filePath,
            attachment.Content,
            cancellationToken);

        Process.Start(
            new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
    }

    public bool ConfirmDelete(
        MeetingAttachmentItem attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var result =
            MessageBox.Show(
                $"Delete \"{attachment.OriginalFileName}\"?\n\n" +
                "This will permanently remove the attachment.",
                "Delete Attachment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        return result == MessageBoxResult.Yes;
    }
}
