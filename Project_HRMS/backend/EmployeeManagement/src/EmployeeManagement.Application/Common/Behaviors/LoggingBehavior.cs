using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Common.Behaviors;

/// <summary>Logs request name and duration only. Request payloads are never logged (they may contain personal data).</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestMilliseconds = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > SlowRequestMilliseconds)
                logger.LogWarning("Slow request {RequestName} completed in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
            else
                logger.LogDebug("Request {RequestName} completed in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogDebug("Request {RequestName} failed after {ElapsedMs} ms with {ExceptionType}", name, sw.ElapsedMilliseconds, ex.GetType().Name);
            throw;
        }
    }
}
