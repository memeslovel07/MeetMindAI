using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.MeetingSummaries;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.MeetingSummaries;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.Meetings;



namespace MeetMindAI.WPF.ViewModels.MeetingSummaries;

public partial class MeetingSummaryViewModel
    : ViewModelBase, INavigationAware
{
    private readonly IMeetingSummaryApiService _summaryApiService;
    private readonly INavigationService _navigationService;

    private Guid _meetingId;

    [ObservableProperty]
    private MeetingSummaryDetails? _summary;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegenerateCommand))]
    private bool _summaryExists;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegenerateCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegenerateCommand))]
    private bool _isGenerating;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public MeetingSummaryViewModel(
        IMeetingSummaryApiService summaryApiService,
        INavigationService navigationService)
    {
        _summaryApiService = summaryApiService;
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

            Summary =
                await _summaryApiService.GetAsync(
                    _meetingId,
                    cancellationToken);

            SummaryExists = Summary is not null;
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

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync()
    {
        if (_meetingId == Guid.Empty ||
            IsGenerating)
        {
            return;
        }

        try
        {
            IsGenerating = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await _summaryApiService.GenerateAsync(
                _meetingId);

            // GET again because it contains the complete
            // Provider/Model/PromptVersion metadata.
            Summary =
                await _summaryApiService.GetAsync(
                    _meetingId);

            SummaryExists = Summary is not null;

            SuccessMessage =
                "AI summary generated successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRegenerate))]
    private async Task RegenerateAsync()
    {
        if (_meetingId == Guid.Empty ||
            !SummaryExists ||
            IsGenerating)
        {
            return;
        }

        try
        {
            IsGenerating = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await _summaryApiService.RegenerateAsync(
                _meetingId);

            Summary =
                await _summaryApiService.GetAsync(
                    _meetingId);

            SummaryExists = Summary is not null;

            SuccessMessage =
                "AI summary regenerated successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsGenerating = false;
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

    private bool CanGenerate()
    {
        return _meetingId != Guid.Empty &&
               !SummaryExists &&
               !IsGenerating &&
               !IsLoading;
    }

    private bool CanRegenerate()
    {
        return _meetingId != Guid.Empty &&
               SummaryExists &&
               !IsGenerating &&
               !IsLoading;
    }
}
