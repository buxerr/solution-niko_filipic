using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception, "An exception occurred after the response had already started.");
                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(context, exception);
        var title = GetTitle(statusCode);
        var detail = GetDetail(statusCode, exception);

        LogException(exception, statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static int GetStatusCode(HttpContext context, Exception exception)
    {
        return exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            HttpRequestException => StatusCodes.Status503ServiceUnavailable,
            TaskCanceledException when !context.RequestAborted.IsCancellationRequested =>
                StatusCodes.Status504GatewayTimeout,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status503ServiceUnavailable => "External service unavailable",
            StatusCodes.Status504GatewayTimeout => "External service timeout",
            _ => "Internal server error"
        };
    }

    private static string GetDetail(int statusCode, Exception exception)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return "An unexpected error occurred.";
        }

        return exception.Message;
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
            return;
        }

        _logger.LogWarning(exception, "Handled exception occurred with status code {StatusCode}.", statusCode);
    }
}
