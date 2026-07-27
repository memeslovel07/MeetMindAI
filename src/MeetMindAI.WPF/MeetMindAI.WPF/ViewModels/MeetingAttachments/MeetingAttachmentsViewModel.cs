using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.MeetingAttachments;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.Dialogs;
using MeetMindAI.WPF.Services.MeetingAttachments;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.Meetings;



namespace MeetMindAI.WPF.ViewModels.MeetingAttachments;

public partial class MeetingAttachmentsViewModel
    : ViewModelBase, INavigationAware
{
    private readonly IMeetingAttachmentApiService
        _attachmentApiService;

    private readonly IAttachmentDialogService
        _dialogService;

    private readonly INavigationService
        _navigationService;

    private Guid _meetingId;

    public ObservableCollection<MeetingAttachmentItem> Attachments { get; }
        = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isUploading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public bool HasAttachments =>
        Attachments.Count > 0;

    public int AttachmentCount =>
        Attachments.Count;

    public MeetingAttachmentsViewModel(
        IMeetingAttachmentApiService attachmentApiService,
        IAttachmentDialogService dialogService,
        INavigationService navigationService)
    {
        _attachmentApiService = attachmentApiService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken = default)
    {
        if (parameter is not Guid meetingId ||
            meetingId == Guid.Empty)
        {
            ErrorMessage =
                "A valid meeting identifier is required.";

            return;
        }

        _meetingId = meetingId;

        RefreshCommand.NotifyCanExecuteChanged();
        UploadCommand.NotifyCanExecuteChanged();
        OpenCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();

        await LoadAsync(
            cancellationToken);
    }

    private async Task LoadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var attachments =
                await _attachmentApiService.GetAllAsync(
                    _meetingId,
                    cancellationToken);

            Attachments.Clear();

            foreach (var attachment in attachments
                         .OrderByDescending(
                             x => x.CreatedAtUtc))
            {
                Attachments.Add(
                    attachment);
            }

            NotifyStatistics();
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage =
                "Your session has expired. Please sign in again.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Unable to connect to the MeetMindAI server.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        SuccessMessage = null;

        await LoadAsync();
    }

    [RelayCommand]
    private async Task UploadAsync()
    {
        if (_meetingId == Guid.Empty ||
            IsUploading)
        {
            return;
        }

        var filePath =
            _dialogService.SelectFile();

        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }

        try
        {
            IsUploading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var uploaded =
                await _attachmentApiService.UploadAsync(
                    _meetingId,
                    filePath);

            await LoadAsync();

            SuccessMessage =
                $"\"{uploaded.OriginalFileName}\" uploaded successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage =
                "Your session has expired. Please sign in again.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsUploading = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(
        MeetingAttachmentItem? attachment)
    {
        if (attachment is null ||
            IsProcessing)
        {
            return;
        }

        try
        {
            IsProcessing = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var downloaded =
                await _attachmentApiService.DownloadAsync(
                    _meetingId,
                    attachment.Id);

            await _dialogService.SaveAndOpenAsync(
                downloaded);
        }
        catch (FileNotFoundException)
        {
            ErrorMessage =
                "The attachment file could not be found.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(
    MeetingAttachmentItem? attachment)
    {
        if (attachment is null ||
            IsProcessing)
        {
            return;
        }

        if (!_dialogService.ConfirmDelete(
                attachment))
        {
            return;
        }

        try
        {
            IsProcessing = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await _attachmentApiService.DeleteAsync(
                _meetingId,
                attachment.Id);

            Attachments.Remove(
                attachment);

            NotifyStatistics();

            SuccessMessage =
                "Attachment deleted successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task BackToMeetingAsync()
    {
        if (_meetingId == Guid.Empty)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<MeetingDetailsViewModel>(
                _meetingId);
    }

    private void NotifyStatistics()
    {
        OnPropertyChanged(
            nameof(HasAttachments));

        OnPropertyChanged(
            nameof(AttachmentCount));
    }

    private bool CanRefresh()
    {
        return _meetingId != Guid.Empty &&
               !IsLoading &&
               !IsUploading &&
               !IsProcessing;
    }

    private bool CanUpload()
    {
        return _meetingId != Guid.Empty &&
               !IsLoading &&
               !IsUploading &&
               !IsProcessing;
    }

    private bool CanOpen(
        MeetingAttachmentItem? attachment)
    {
        return attachment is not null &&
               _meetingId != Guid.Empty &&
               !IsLoading &&
               !IsUploading &&
               !IsProcessing;
    }

    private bool CanDelete(
        MeetingAttachmentItem? attachment)
    {
        return attachment is not null &&
               _meetingId != Guid.Empty &&
               !IsLoading &&
               !IsUploading &&
               !IsProcessing;
    }
}
