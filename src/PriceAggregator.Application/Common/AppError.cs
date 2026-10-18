namespace PriceAggregator.Application.Common;

public sealed record AppError(ErrorKind Kind, string Code, string Message);
