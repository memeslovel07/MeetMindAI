using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.Transcripts;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.Transcripts;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.Meetings;



namespace MeetMindAI.WPF.ViewModels.Transcripts;

public partial class TranscriptViewModel
    : ViewModelBase, INavigationAware
{
    private readonly ITranscriptApiService _transcriptApiService;

    private readonly INavigationService _navigationService;

    private Guid _meetingId;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string? _language = "English";

    [ObservableProperty]
    private int? _durationSeconds;

    [ObservableProperty]
    private bool _transcriptExists;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public TranscriptViewModel(
       ITranscriptApiService transcriptApiService,
       INavigationService navigationService)
    {
        _transcriptApiService = transcriptApiService;
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

        SaveCommand.NotifyCanExecuteChanged();

        await LoadAsync(cancellationToken);
    }

    private async Task LoadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var transcript =
                await _transcriptApiService.GetAsync(
                    _meetingId,
                    cancellationToken);

            if (transcript is null)
            {
                TranscriptExists = false;

                Content = string.Empty;
                Language = "English";
                DurationSeconds = null;

                return;
            }

            TranscriptExists = true;

            Content = transcript.Content;
            Language = transcript.Language;

            DurationSeconds = transcript.Duration.HasValue
                ? (int)transcript.Duration.Value.TotalSeconds
                : null;
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
        catch (Exception)
        {
            ErrorMessage =
                "Unable to load the transcript.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {

        if (_meetingId == Guid.Empty ||
    IsSaving ||
    IsLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            ErrorMessage =
                "Transcript content is required.";

            return;
        }

        if (DurationSeconds.HasValue &&
            DurationSeconds <= 0)
        {
            ErrorMessage =
                "Duration must be greater than zero.";

            return;
        }

        try
        {
            IsSaving = true;

            ErrorMessage = null;
            SuccessMessage = null;

            if (TranscriptExists)
            {
                var request =
                    new UpdateTranscriptRequest(
                        Content.Trim(),
                        string.IsNullOrWhiteSpace(Language)
                            ? null
                            : Language.Trim(),
                        DurationSeconds);

                await _transcriptApiService.UpdateAsync(
                    _meetingId,
                    request);

                SuccessMessage =
                    "Transcript updated successfully.";
            }
            else
            {
                var request =
                    new CreateTranscriptRequest(
                        Content.Trim(),
                        string.IsNullOrWhiteSpace(Language)
                            ? null
                            : Language.Trim(),
                        DurationSeconds);

                await _transcriptApiService.CreateAsync(
                    _meetingId,
                    request);

                TranscriptExists = true;

                SuccessMessage =
                    "Transcript created successfully.";
            }
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
            IsSaving = false;
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

    private bool CanSave()
    {
        return _meetingId != Guid.Empty &&
               !IsLoading &&
               !IsSaving;
    }
}
