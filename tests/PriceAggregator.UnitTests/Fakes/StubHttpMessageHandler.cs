using System.Net;
using System.Text;

namespace PriceAggregator.UnitTests.Fakes;

internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
{
    public Uri? LastRequestUri { get; private set; }

    public HttpClient CreateClient(string baseAddress) => new(this) { BaseAddress = new Uri(baseAddress) };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });
    }
}
