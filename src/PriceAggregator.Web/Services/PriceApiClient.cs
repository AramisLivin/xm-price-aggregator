using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using PriceAggregator.Contracts;

namespace PriceAggregator.Web.Services;

internal sealed class PriceApiClient(HttpClient httpClient)
{
    private const string ProblemMediaType = "application/problem+json";

    private static readonly ApiError UnexpectedResponse =
        new("Unexpected response", "Could not read the API response");

    public Task<ApiResult<InstrumentsResponse>> GetInstrumentsAsync() =>
        GetAsync<InstrumentsResponse>(ApiRoutes.Instruments);

    public Task<ApiResult<AggregatedPriceResponse>> GetPriceAsync(
        string instrument,
        DateTimeOffset timestamp) =>
        GetAsync<AggregatedPriceResponse>(
            $"{ApiRoutes.Prices}/{ApiRoutes.ToRouteSegment(instrument)}?timestamp={FormatTimestamp(timestamp)}");

    public Task<ApiResult<PriceHistoryResponse>> GetHistoryAsync(
        string instrument,
        DateTimeOffset from,
        DateTimeOffset to) =>
        GetAsync<PriceHistoryResponse>(
            $"{ApiRoutes.Prices}/{ApiRoutes.ToRouteSegment(instrument)}/history?from={FormatTimestamp(from)}&to={FormatTimestamp(to)}");

    private async Task<ApiResult<T>> GetAsync<T>(string requestUri)
    {
        try
        {
            using var response = await httpClient.GetAsync(new Uri(requestUri, UriKind.Relative));
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult.Failure<T>(await ReadErrorAsync(response));
            }

            var value = await response.Content.ReadFromJsonAsync<T>();
            return value is null ? ApiResult.Failure<T>(UnexpectedResponse) : ApiResult.Success(value);
        }
        catch (HttpRequestException)
        {
            return ApiResult.Failure<T>(new ApiError(
                "API is unreachable",
                $"Can't connect to {httpClient.BaseAddress}"));
        }
        catch (JsonException)
        {
            return ApiResult.Failure<T>(UnexpectedResponse);
        }
    }

    private static async Task<ApiError> ReadErrorAsync(HttpResponseMessage response)
    {
        var fallback = new ApiError(
            $"Request failed ({(int)response.StatusCode})",
            response.ReasonPhrase ?? "API returned an error");

        if (response.Content.Headers.ContentType?.MediaType != ProblemMediaType)
        {
            return fallback;
        }

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
            return problem?.Detail is null ? fallback : new ApiError(problem.Title ?? fallback.Title, problem.Detail);
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private static string FormatTimestamp(DateTimeOffset timestamp) =>
        Uri.EscapeDataString(timestamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
}
