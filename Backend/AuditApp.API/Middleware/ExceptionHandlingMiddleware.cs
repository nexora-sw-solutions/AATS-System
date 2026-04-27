using System.Net;
using System.Text.Json;
using AuditApp.Application.Common;

namespace AuditApp.API.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND"),
            ArgumentException => (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),
            InvalidOperationException => (HttpStatusCode.Conflict, "CONFLICT"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR")
        };

        var response = ApiResponse<object>.Fail(errorCode, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
