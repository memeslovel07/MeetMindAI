using CommunityToolkit.Mvvm.ComponentModel;


using MeetMindAI.WPF.ViewModels.ActionItems;
using MeetMindAI.WPF.ViewModels.MeetingSummaries;
using CommunityToolkit.Mvvm.Input;
using MeetMindAI.WPF.ViewModels.Transcripts;
using MeetMindAI.WPF.Models.Meetings;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.Meetings;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.MeetingAttachments;

namespace MeetMindAI.WPF.ViewModels.Meetings;

public partial class MeetingDetailsViewModel
    : ViewModelBase, INavigationAware
{
    private readonly IMeetingApiService _meetingApiService;

    private readonly INavigationService _navigationService;



    [ObservableProperty]
    private MeetingDetails? _meeting;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public MeetingDetailsViewModel(
    IMeetingApiService meetingApiService,
    INavigationService navigationService)
    {
        _meetingApiService = meetingApiService;
        _navigationService = navigationService;
    }

    public async Task LoadAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Meeting =
                await _meetingApiService.GetByIdAsync(
                    meetingId,
                    cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage =
                "Your session has expired. Please sign in again.";
        }
        catch (System.Net.Http.HttpRequestException)
        {
            ErrorMessage =
                "Unable to connect to the MeetMindAI server.";
        }
        catch (Exception)
        {
            ErrorMessage =
                "Unable to load the meeting.";
        }
        finally
        {
            IsLoading = false;
        }
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

        await LoadAsync(
            meetingId,
            cancellationToken);
    }

    [RelayCommand]
    private async Task OpenTranscriptAsync()
    {
        if (Meeting is null)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<TranscriptViewModel>(
                Meeting.Id);
    }

    [RelayCommand]
    private async Task OpenSummaryAsync()
    {
        if (Meeting is null)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<MeetingSummaryViewModel>(
                Meeting.Id);
    }

    [RelayCommand]
    private async Task OpenActionItemsAsync()
    {
        if (Meeting is null)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<ActionItemsViewModel>(
                Meeting.Id);
    }

    [RelayCommand]
    private async Task OpenAttachmentsAsync()
    {
        if (Meeting is null)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<MeetingAttachmentsViewModel>(
                Meeting.Id);
    }

    [RelayCommand]
    private void BackToMeetings()
    {
        _navigationService
            .NavigateTo<MeetingsViewModel>();
    }
}
