using MeetMindAI.WPF.ViewModels.Base;

using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.WPF.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ViewModelBase? CurrentViewModel { get; private set; }

    public event EventHandler<ViewModelBase>? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase
    {
        var viewModel =
            _serviceProvider.GetRequiredService<TViewModel>();

        SetCurrentViewModel(viewModel);
    }

    public async Task NavigateToAsync<TViewModel>(
        object? parameter = null,
        CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase
    {
        var viewModel =
            _serviceProvider.GetRequiredService<TViewModel>();

        SetCurrentViewModel(viewModel);

        if (viewModel is INavigationAware navigationAware)
        {
            await navigationAware.OnNavigatedToAsync(
                parameter,
                cancellationToken);
        }
    }

    private void SetCurrentViewModel(
        ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;

        CurrentViewModelChanged?.Invoke(
            this,
            viewModel);
    }
}
