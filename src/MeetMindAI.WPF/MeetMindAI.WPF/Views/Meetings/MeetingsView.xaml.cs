using System.Windows;
using System.Windows.Controls;

using MeetMindAI.WPF.ViewModels.Meetings;

using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.WPF.Views.Meetings;

public partial class MeetingsView : UserControl
{
    public MeetingsView()
    {
        InitializeComponent();
    }

    private async void NewMeeting_OnClick(
    object sender,
    RoutedEventArgs e)
    {
        var app =
            (App)System.Windows.Application.Current;

        var window =
            app.Services.GetRequiredService<CreateMeetingWindow>();

        window.Owner =
            Window.GetWindow(this);

        var result =
            window.ShowDialog();

        if (result == true &&
            DataContext is MeetingsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
