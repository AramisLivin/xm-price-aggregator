using System.Diagnostics.CodeAnalysis;

namespace PriceAggregator.Application.Common;

public sealed class Result<T>
{
    internal Result(T value)
    {
        Value = value;
    }

    internal Result(AppError error)
    {
        Error = error;
    }

    public T? Value { get; }

    public AppError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(AppError error) => new(error);
}
