using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Telemetry.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;
        if (exception is KeyNotFoundException)
        {
            statusCode = 404;
            message = _env.IsDevelopment() ? exception.Message : "Resource not found.";
        }
        else if (exception is InvalidOperationException)
        {
            statusCode = 409;
            message = _env.IsDevelopment() ? exception.Message : "Conflict.";
        }
        else if (exception is ArgumentException)
        {
            statusCode = 400;
            message = _env.IsDevelopment() ? exception.Message : "Invalid request.";
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
