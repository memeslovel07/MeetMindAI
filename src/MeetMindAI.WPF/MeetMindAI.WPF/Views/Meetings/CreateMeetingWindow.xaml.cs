using System.Windows;

using MeetMindAI.WPF.ViewModels.Meetings;

namespace MeetMindAI.WPF.Views.Meetings;

public partial class CreateMeetingWindow : Window
{
    private readonly CreateMeetingViewModel _viewModel;

    public CreateMeetingWindow(
        CreateMeetingViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;

        _viewModel.MeetingCreated +=
            OnMeetingCreated;

        _viewModel.CancelRequested +=
            OnCancelRequested;
    }

    private void OnMeetingCreated(
        object? sender,
        EventArgs e)
    {
        DialogResult = true;

        Close();
    }

    private void OnCancelRequested(
        object? sender,
        EventArgs e)
    {
        DialogResult = false;

        Close();
    }

    protected override void OnClosed(
        EventArgs e)
    {
        _viewModel.MeetingCreated -=
            OnMeetingCreated;

        _viewModel.CancelRequested -=
            OnCancelRequested;

        base.OnClosed(e);
    }
}
