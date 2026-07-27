using System.Collections.ObjectModel;
using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.Meetings;
using MeetMindAI.WPF.Services.Meetings;
using MeetMindAI.WPF.ViewModels.Base;



namespace MeetMindAI.WPF.ViewModels.Dashboard;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IMeetingApiService _meetingApiService;

    public ObservableCollection<MeetingListItem> RecentMeetings { get; }
        = new();

    [ObservableProperty]
    private int _totalMeetings;

    [ObservableProperty]
    private int _scheduledMeetings;

    [ObservableProperty]
    private int _completedMeetings;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public DashboardViewModel(
        IMeetingApiService meetingApiService)
    {
        _meetingApiService = meetingApiService;
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
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

            TotalMeetings = meetings.Count;

            ScheduledMeetings = meetings.Count(
                x => x.Status == MeetingStatus.Scheduled);

            CompletedMeetings = meetings.Count(
                x => x.Status == MeetingStatus.Completed);

            RecentMeetings.Clear();

            foreach (var meeting in meetings
                         .OrderByDescending(x => x.ScheduledAtUtc)
                         .Take(5))
            {
                RecentMeetings.Add(meeting);
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
        catch (Exception)
        {
            ErrorMessage =
                "Unable to load dashboard data.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLoad()
    {
        return !IsLoading;
    }
}
