using System.Windows;

using MeetMindAI.WPF.ViewModels.Authentication;
using MeetMindAI.WPF.ViewModels.Shared;
using MeetMindAI.WPF.Views.Authentication;
using MeetMindAI.WPF.Views.Shared;

namespace MeetMindAI.WPF.Views;

public partial class MainWindow : Window
{
    private readonly LoginView _loginView;
    private readonly LoginViewModel _loginViewModel;

    private readonly ShellView _shellView;
    private readonly ShellViewModel _shellViewModel;

    public MainWindow(
      LoginView loginView,
      LoginViewModel loginViewModel,
      ShellView shellView,
      ShellViewModel shellViewModel)
    {
        InitializeComponent();

        _loginView = loginView;
        _loginViewModel = loginViewModel;
        _shellView = shellView;
        _shellViewModel = shellViewModel;

        _loginViewModel.LoginSucceeded +=
            HandleLoginSucceeded;

        _shellViewModel.LogoutRequested +=
            OnLogoutRequested;

        ShowLoginView();
    }

    private void ShowLoginView()
    {
        _loginViewModel.Reset();

        _loginView.DataContext = _loginViewModel;

        MainContent.Content = _loginView;
    }

    private async void HandleLoginSucceeded(
    object? sender,
    EventArgs e)
    {
        _shellView.DataContext =
            _shellViewModel;

        MainContent.Content =
            _shellView;

        await _shellViewModel.InitializeAsync();
    }

    private void OnLogoutRequested(
    object? sender,
    EventArgs e)
    {
        ShowLoginView();
    }

}
