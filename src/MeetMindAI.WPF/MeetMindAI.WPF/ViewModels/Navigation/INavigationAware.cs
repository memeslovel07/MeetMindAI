namespace MeetMindAI.WPF.Navigation;

public interface INavigationAware
{
    Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken = default);
}
