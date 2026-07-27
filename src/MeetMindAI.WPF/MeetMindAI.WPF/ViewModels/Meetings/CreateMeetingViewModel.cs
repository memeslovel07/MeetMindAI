using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.Meetings;
using MeetMindAI.WPF.Services.Meetings;
using MeetMindAI.WPF.ViewModels.Base;

using System.Globalization;

namespace MeetMindAI.WPF.ViewModels.Meetings;

public partial class CreateMeetingViewModel : ViewModelBase
{
    private readonly IMeetingApiService _meetingApiService;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private DateTime? _scheduledDate = DateTime.Today;

    [ObservableProperty]
    private string _scheduledTime = "10:00";

    [ObservableProperty]
    private int _durationMinutes = 30;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public event EventHandler? MeetingCreated;

    public event EventHandler? CancelRequested;

    public CreateMeetingViewModel(
        IMeetingApiService meetingApiService)
    {
        _meetingApiService = meetingApiService;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {

        if (IsLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Meeting title is required.";
            return;
        }

        if (DurationMinutes <= 0)
        {
            ErrorMessage =
                "Duration must be greater than zero.";
            return;
        }

        DateTime? scheduledAtUtc = null;

        if (ScheduledDate.HasValue)
        {
            if (!TimeSpan.TryParseExact(
        ScheduledTime,
        @"hh\:mm",
        CultureInfo.InvariantCulture,
        out var scheduledTime))
            {
                ErrorMessage =
                    "Enter time in HH:mm format.";
                return;
            }

            var localDateTime =
                ScheduledDate.Value.Date
                    .Add(scheduledTime);

            scheduledAtUtc =
                localDateTime.ToUniversalTime();
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var request = new CreateMeetingRequest(
                Title.Trim(),
                string.IsNullOrWhiteSpace(Description)
                    ? null
                    : Description.Trim(),
                scheduledAtUtc,
                DurationMinutes);

            await _meetingApiService.CreateAsync(request);

            MeetingCreated?.Invoke(
                this,
                EventArgs.Empty);
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
                "Unable to create the meeting.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        CancelRequested?.Invoke(
            this,
            EventArgs.Empty);
    }

    private bool CanCreate()
    {
        return !IsLoading;
    }

    private bool CanCancel()
    {
        return !IsLoading;
    }
}
