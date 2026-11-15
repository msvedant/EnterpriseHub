namespace EmployeeManagement.API.Models;

/// <summary>Uniform envelope for every API response (success and error).</summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null, string? traceId = null) =>
        new() { Success = false, Message = message, Errors = errors, TraceId = traceId };
}
