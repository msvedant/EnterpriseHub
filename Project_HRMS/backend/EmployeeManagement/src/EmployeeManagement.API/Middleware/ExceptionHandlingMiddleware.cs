using EmployeeManagement.API.Models;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Middleware;

/// <summary>Maps exceptions to the consistent <see cref="ApiResponse{T}"/> error format.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request was cancelled by the client");
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, message, errors) = Map(ex);

        if (status >= 500)
            logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Request failed with {StatusCode}: {Message}", status, message);

        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(message, errors, context.TraceIdentifier));
    }

    private static (int Status, string Message, Dictionary<string, string[]>? Errors) Map(Exception ex) => ex switch
    {
        ValidationException v => (StatusCodes.Status400BadRequest, "One or more validation errors occurred.", GroupErrors(v)),
        NotFoundException e => (StatusCodes.Status404NotFound, e.Message, null),
        ConflictException e => (StatusCodes.Status409Conflict, e.Message, null),
        UnauthorizedException e => (StatusCodes.Status401Unauthorized, e.Message, null),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
            "The record was modified or deleted by another user. Reload it and try again.", null),
        DbUpdateException d when DbExceptionHelper.IsUniqueViolation(d) => (StatusCodes.Status409Conflict,
            "A record with the same unique value already exists.", null),
        DbUpdateException d when DbExceptionHelper.IsForeignKeyViolation(d) => (StatusCodes.Status409Conflict,
            "The operation conflicts with related data.", null),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "The request is malformed.", null),
        _ => (StatusCodes.Status500InternalServerError,
            "An unexpected error occurred. Please contact support and quote the trace id.", null)
    };

    private static Dictionary<string, string[]> GroupErrors(ValidationException ex) =>
        ex.Errors
            .GroupBy(e => NormalizePath(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

    /// <summary>"Data.Families[0].FullName" → "families[0].fullName".</summary>
    private static string NormalizePath(string property)
    {
        if (string.IsNullOrWhiteSpace(property))
            return "request";

        if (property.StartsWith("Data.", StringComparison.Ordinal))
            property = property[5..];

        return string.Join('.', property.Split('.').Select(s => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..]));
    }
}
