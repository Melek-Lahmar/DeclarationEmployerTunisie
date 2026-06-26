using System.Net.Http;
using System.Net.Http.Headers;

namespace DeclarationEmployer.Desktop.Services;

public sealed class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly SessionService _sessionService;

    public AuthorizationHeaderHandler(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_sessionService.IsAuthenticated && !string.IsNullOrWhiteSpace(_sessionService.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
