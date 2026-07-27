using MeetMindAI.WPF.Models.Authentication;

namespace MeetMindAI.WPF.Services.Authentication;

public interface IAuthenticationApiService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
