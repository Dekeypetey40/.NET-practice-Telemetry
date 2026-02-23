using System.Net;
using System.Text.Json;

namespace Telemetry.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static async Task HandleAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;
        if (exception is KeyNotFoundException)
        {
            statusCode = 404;
            message = exception.Message;
        }
        else if (exception is InvalidOperationException)
        {
            statusCode = 409;
            message = exception.Message;
        }
        else if (exception is ArgumentException)
        {
            statusCode = 400;
            message = exception.Message;
        }
        else
        {
            statusCode = 500;
            message = "An error occurred.";
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(body);
    }
}
