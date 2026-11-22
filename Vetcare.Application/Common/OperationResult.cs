namespace Vetcare.Application.Common;

public sealed record OperationResult<T>(
    int StatusCode,
    T? Value = default,
    Dictionary<string, string[]>? Errors = null,
    string? Message = null);
