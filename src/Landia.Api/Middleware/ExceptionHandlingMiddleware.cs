using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Landia.Api.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private ILogger<ExceptionHandlingMiddleware> Logger { get; }

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro não tratado na requisição {Method} {Path}", context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ArgumentException or ValidationException => new ErrorResponse
            {
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.BadRequest
            },
            InvalidOperationException => new ErrorResponse
            {
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.Conflict
            },
            KeyNotFoundException => new ErrorResponse
            {
                Message = "Recurso não encontrado",
                StatusCode = (int)HttpStatusCode.NotFound
            },
            _ => new ErrorResponse
            {
                Message = "Erro interno do servidor",
                StatusCode = (int)HttpStatusCode.InternalServerError
            }
        };

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}