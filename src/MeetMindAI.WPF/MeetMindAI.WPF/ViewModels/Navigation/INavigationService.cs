using MeetMindAI.WPF.ViewModels.Base;

namespace MeetMindAI.WPF.Navigation;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    event EventHandler<ViewModelBase>? CurrentViewModelChanged;

    void NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase;

    Task NavigateToAsync<TViewModel>(
        object? parameter = null,
        CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase;
}
