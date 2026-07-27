using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.ActionItems;
using MeetMindAI.WPF.Services.ActionItems;
using MeetMindAI.WPF.ViewModels.Base;

using Microsoft.VisualBasic;

using Xunit.Abstractions;

namespace MeetMindAI.WPF.ViewModels.ActionItems;

public partial class ActionItemEditorViewModel
    : ViewModelBase
{
    private readonly IActionItemApiService _actionItemApiService;

    private Guid _meetingId;
    private Guid? _actionItemId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private ActionItemPriority _priority =
        ActionItemPriority.Medium;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    public IReadOnlyList<ActionItemPriority> Priorities { get; } =
        Enum.GetValues<ActionItemPriority>();

    public bool IsEditMode =>
        _actionItemId.HasValue;

    public string WindowTitle =>
        IsEditMode
            ? "Edit Action Item"
            : "Create Action Item";

    public string SaveButtonText =>
        IsEditMode
            ? "Save Changes"
            : "Create Action Item";

    public event EventHandler? Saved;

    public event EventHandler? Cancelled;

    public ActionItemEditorViewModel(
        IActionItemApiService actionItemApiService)
    {
        _actionItemApiService = actionItemApiService;
    }

    public void InitializeForCreate(
        Guid meetingId)
    {
        _meetingId = meetingId;
        _actionItemId = null;

        Title = string.Empty;
        Description = null;
        Priority = ActionItemPriority.Medium;
        DueDate = null;
        ErrorMessage = null;

        NotifyModeProperties();
    }

    public void InitializeForEdit(
        ActionItemDetails actionItem)
    {
        ArgumentNullException.ThrowIfNull(actionItem);

        _meetingId = actionItem.MeetingId;
        _actionItemId = actionItem.Id;

        Title = actionItem.Title;
        Description = actionItem.Description;
        Priority = actionItem.Priority;
        DueDate = actionItem.DueDateLocal?.Date;
        ErrorMessage = null;

        NotifyModeProperties();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage =
                "Action item title is required.";

            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = null;

            if (_actionItemId.HasValue)
            {
                var request =
                    new UpdateActionItemRequest(
                        Title.Trim(),
                        NormalizeDescription(),
                        Priority,
                        ConvertDueDateToUtc());

                await _actionItemApiService.UpdateAsync(
                    _actionItemId.Value,
                    request);
            }
            else
            {
                var request =
                    new CreateActionItemRequest(
                        Title.Trim(),
                        NormalizeDescription(),
                        Priority,
                        ConvertDueDateToUtc());

                await _actionItemApiService.CreateAsync(
                    _meetingId,
                    request);
            }

            Saved?.Invoke(
                this,
                EventArgs.Empty);
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
    private void Cancel()
    {
        Cancelled?.Invoke(
            this,
            EventArgs.Empty);
    }

    private string? NormalizeDescription()
    {
        return string.IsNullOrWhiteSpace(Description)
            ? null
            : Description.Trim();
    }

    private DateTime? ConvertDueDateToUtc()
    {
        if (!DueDate.HasValue)
        {
            return null;
        }

        var localDate =
            DateTime.SpecifyKind(
                DueDate.Value.Date,
                DateTimeKind.Local);

        return localDate.ToUniversalTime();
    }

    private void NotifyModeProperties()
    {
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }
}
