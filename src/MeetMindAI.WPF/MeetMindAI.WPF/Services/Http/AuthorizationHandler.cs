using System.Net.Http;
using System.Net.Http.Headers;

using MeetMindAI.WPF.Services.Authentication;

namespace MeetMindAI.WPF.Services.Http;

public sealed class AuthorizationHandler
    : DelegatingHandler
{
    private readonly IAuthenticationSession _authenticationSession;

    public AuthorizationHandler(
        IAuthenticationSession authenticationSession)
    {
        _authenticationSession = authenticationSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken =
            _authenticationSession.AccessToken;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
