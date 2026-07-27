using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.Authentication;
using MeetMindAI.WPF.ViewModels.Base;
using MeetMindAI.WPF.ViewModels.Dashboard;
using MeetMindAI.WPF.ViewModels.Meetings;

namespace MeetMindAI.WPF.ViewModels.Shared;

public partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationSession _authenticationSession;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public event EventHandler? LogoutRequested;

    public ShellViewModel(
        INavigationService navigationService,
        IAuthenticationSession authenticationSession)
    {
        _navigationService = navigationService;
        _authenticationSession = authenticationSession;

        _navigationService.CurrentViewModelChanged +=
            OnCurrentViewModelChanged;

        CurrentViewModel =
            _navigationService.CurrentViewModel;
    }

    public async Task InitializeAsync()
    {
        _navigationService.NavigateTo<DashboardViewModel>();

        if (CurrentViewModel is DashboardViewModel dashboardViewModel)
        {
            await dashboardViewModel.LoadAsync();
        }
    }

    private void OnCurrentViewModelChanged(
        object? sender,
        ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }

    [RelayCommand]
    private void Dashboard()
    {
        if (CurrentViewModel is DashboardViewModel)
        {
            return;
        }

        _navigationService.NavigateTo<DashboardViewModel>();
    }

    [RelayCommand]
    private void Logout()
    {
        _authenticationSession.Clear();

        LogoutRequested?.Invoke(
            this,
            EventArgs.Empty);
    }

    [RelayCommand]
    private async Task MeetingsAsync()
    {
        if (CurrentViewModel is MeetingsViewModel)
        {
            return;
        }

        _navigationService.NavigateTo<MeetingsViewModel>();

        if (CurrentViewModel is MeetingsViewModel meetingsViewModel &&
            meetingsViewModel.Meetings.Count == 0)
        {
            await meetingsViewModel.LoadAsync();
        }
    }
}
