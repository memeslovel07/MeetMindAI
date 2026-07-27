using System.Collections.ObjectModel;
using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.Meetings;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.Meetings;
using MeetMindAI.WPF.ViewModels.Base;



namespace MeetMindAI.WPF.ViewModels.Meetings;

public partial class MeetingsViewModel : ViewModelBase
{
    private readonly IMeetingApiService _meetingApiService;
    private readonly INavigationService _navigationService;

    public ObservableCollection<MeetingListItem> Meetings { get; }
        = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenMeetingCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasMeetings =>
        Meetings.Count > 0;

    public MeetingsViewModel(
     IMeetingApiService meetingApiService,
     INavigationService navigationService)
    {
        _meetingApiService = meetingApiService;
        _navigationService = navigationService;
    }

    public async Task LoadAsync()
    {

        if (IsLoading)
        {
            return;
        }


        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var meetings =
                await _meetingApiService.GetMineAsync();

            Meetings.Clear();

            foreach (var meeting in meetings
                         .OrderByDescending(x => x.ScheduledAtUtc))
            {
                Meetings.Add(meeting);
            }

            OnPropertyChanged(nameof(HasMeetings));
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
                "Unable to load your meetings.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanOpenMeeting))]
    private async Task OpenMeetingAsync(
    MeetingListItem? meeting)
    {
        if (meeting is null)
        {
            return;
        }

        await _navigationService
            .NavigateToAsync<MeetingDetailsViewModel>(
                meeting.Id);
    }

    private bool CanRefresh()
    {
        return !IsLoading;
    }

    private bool CanOpenMeeting(
        MeetingListItem? meeting)
    {
        return meeting is not null &&
               !IsLoading;
    }
}
