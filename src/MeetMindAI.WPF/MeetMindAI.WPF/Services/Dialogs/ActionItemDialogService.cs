using System.Windows;

using MeetMindAI.WPF.Models.ActionItems;
using MeetMindAI.WPF.ViewModels.ActionItems;
using MeetMindAI.WPF.Views.ActionItems;

using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.WPF.Services.Dialogs;

public sealed class ActionItemDialogService
    : IActionItemDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public ActionItemDialogService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool ShowCreate(
        Guid meetingId)
    {
        var viewModel =
            _serviceProvider.GetRequiredService<ActionItemEditorViewModel>();

        viewModel.InitializeForCreate(meetingId);

        var window =
            new ActionItemEditorWindow(viewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

        return window.ShowDialog() == true;
    }

    public bool ShowEdit(
        ActionItemDetails actionItem)
    {
        ArgumentNullException.ThrowIfNull(actionItem);

        var viewModel =
            _serviceProvider.GetRequiredService<ActionItemEditorViewModel>();

        viewModel.InitializeForEdit(actionItem);

        var window =
            new ActionItemEditorWindow(viewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

        return window.ShowDialog() == true;
    }

    public bool ConfirmDelete(
        ActionItemDetails actionItem)
    {
        ArgumentNullException.ThrowIfNull(actionItem);

        var result =
            MessageBox.Show(
                $"Delete \"{actionItem.Title}\"?\n\nThis action cannot be undone.",
                "Delete Action Item",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        return result == MessageBoxResult.Yes;
    }
}
