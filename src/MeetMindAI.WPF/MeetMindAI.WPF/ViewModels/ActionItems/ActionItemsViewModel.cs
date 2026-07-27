using System.Collections.ObjectModel;
using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Services.Dialogs;
using MeetMindAI.WPF.Models.ActionItems;
using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.ActionItems;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.Meetings;



namespace MeetMindAI.WPF.ViewModels.ActionItems;

public partial class ActionItemsViewModel
    : ViewModelBase, INavigationAware
{
    private readonly IActionItemApiService _actionItemApiService;
    private readonly INavigationService _navigationService;
    private readonly IActionItemDialogService _dialogService;

    private Guid _meetingId;

    public ObservableCollection<ActionItemDetails> ActionItems { get; }
        = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExtractCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExtractCommand))]
    private bool _isExtracting;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;


   

    public bool HasActionItems =>
        ActionItems.Count > 0;

    public int TotalActionItems =>
        ActionItems.Count;

    public int PendingActionItems =>
        ActionItems.Count(x =>
            x.Status == ActionItemStatus.Pending);

    public int CompletedActionItems =>
        ActionItems.Count(x =>
            x.Status == ActionItemStatus.Completed);

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

        ExtractCommand.NotifyCanExecuteChanged();

        await LoadAsync(cancellationToken);
    }

    public ActionItemsViewModel(
    IActionItemApiService actionItemApiService,
    INavigationService navigationService,
    IActionItemDialogService dialogService)
    {
        _actionItemApiService = actionItemApiService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    private async Task LoadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var items =
                await _actionItemApiService.GetByMeetingAsync(
                    _meetingId,
                    cancellationToken);

            ActionItems.Clear();

            foreach (var item in items
                         .OrderBy(x => x.Status)
                         .ThenByDescending(x => x.Priority)
                         .ThenBy(x => x.DueDate))
            {
                ActionItems.Add(item);
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

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SuccessMessage = null;

        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanExtract))]
    private async Task ExtractAsync()
    {
        if (_meetingId == Guid.Empty ||
            IsExtracting)
        {
            return;
        }

        try
        {
            IsExtracting = true;

            ErrorMessage = null;
            SuccessMessage = null;

            var createdIds =
                await _actionItemApiService.ExtractAsync(
                    _meetingId);

            await LoadAsync();

            SuccessMessage = createdIds.Count switch
            {
                0 => "AI found no actionable tasks in the transcript.",

                1 => "AI extracted 1 action item.",

                _ => $"AI extracted {createdIds.Count} action items."
            };
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
            IsExtracting = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteAsync(
    ActionItemDetails? actionItem)
    {
        if (actionItem is null ||
            actionItem.Status == ActionItemStatus.Completed ||
            IsProcessing)
        {
            return;
        }

        try
        {
            IsProcessing = true;
            ErrorMessage = null;
            SuccessMessage = null;

            

            await _actionItemApiService.CompleteAsync(
                actionItem.Id);

            await LoadAsync();

            SuccessMessage =
                "Action item completed successfully.";
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
    ActionItemDetails? actionItem)
    {
        if (actionItem is null ||
    IsProcessing)
        {
            return;
        }

        var confirmed =
            _dialogService.ConfirmDelete(
                actionItem);

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsProcessing = true;

            ErrorMessage = null;
            SuccessMessage = null;

            await _actionItemApiService.DeleteAsync(
                actionItem.Id);

            ActionItems.Remove(actionItem);

            NotifyStatistics();

            SuccessMessage =
                "Action item deleted.";
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
        OnPropertyChanged(nameof(HasActionItems));
        OnPropertyChanged(nameof(TotalActionItems));
        OnPropertyChanged(nameof(PendingActionItems));
        OnPropertyChanged(nameof(CompletedActionItems));
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (_meetingId == Guid.Empty)
        {
            return;
        }

        var created =
            _dialogService.ShowCreate(
                _meetingId);

        if (!created)
        {
            return;
        }

        await LoadAsync();

        SuccessMessage =
            "Action item created successfully.";
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditAsync(
    ActionItemDetails? actionItem)
    {
        if (actionItem is null)
        {
            return;
        }

        var updated =
            _dialogService.ShowEdit(
                actionItem);

        if (!updated)
        {
            return;
        }

        await LoadAsync();

        SuccessMessage =
            "Action item updated successfully.";
    }

    private bool CanEdit(
    ActionItemDetails? actionItem)
    {
        return actionItem is not null &&
               !IsProcessing;
    }

    private bool CanComplete(
        ActionItemDetails? actionItem)
    {
        return actionItem is not null &&
               actionItem.Status != ActionItemStatus.Completed &&
               !IsProcessing;
    }

    private bool CanDelete(
        ActionItemDetails? actionItem)
    {
        return actionItem is not null &&
               !IsProcessing;
    }

    private bool CanExtract()
    {
        return _meetingId != Guid.Empty &&
               !IsExtracting &&
               !IsLoading &&
               !IsProcessing;
    }
}
