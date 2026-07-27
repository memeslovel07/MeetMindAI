using System.Windows;

using MeetMindAI.WPF.ViewModels.ActionItems;

namespace MeetMindAI.WPF.Views.ActionItems;

public partial class ActionItemEditorWindow : Window
{
    public ActionItemEditorWindow(
        ActionItemEditorViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        viewModel.Saved += OnSaved;
        viewModel.Cancelled += OnCancelled;

        Closed += (_, _) =>
        {
            viewModel.Saved -= OnSaved;
            viewModel.Cancelled -= OnCancelled;
        };
    }

    private void OnSaved(
        object? sender,
        EventArgs e)
    {
        DialogResult = true;
    }

    private void OnCancelled(
        object? sender,
        EventArgs e)
    {
        DialogResult = false;
    }
}
