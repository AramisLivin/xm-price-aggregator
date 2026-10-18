using System.Diagnostics.CodeAnalysis;

namespace PriceAggregator.Web.Services;

internal sealed class ApiResult<T>
{
    public ApiResult(T value)
    {
        Value = value;
    }

    public ApiResult(ApiError error)
    {
        Error = error;
    }

    public T? Value { get; }

    public ApiError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;
}

internal static class ApiResult
{
    public static ApiResult<T> Success<T>(T value) => new(value);

    public static ApiResult<T> Failure<T>(ApiError error) => new(error);
}
