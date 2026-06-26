using System.Net;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Common;
using FluentValidation;

namespace DeclarationEmployer.Api.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            var validationErrors = ex.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(error => error.ErrorMessage).ToArray());

            await WriteErrorAsync(
                context,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                "La requete est invalide.",
                validationErrors);
        }
        catch (ApplicationNotFoundException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "NOT_FOUND", ex.Message);
        }
        catch (ApplicationConflictException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, "CONFLICT", ex.Message);
        }
        catch (ApplicationUnauthorizedException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API error");
            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "Une erreur interne est survenue.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string code,
        string message,
        IDictionary<string, string[]>? validationErrors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(new ApiError
        {
            Code = code,
            Message = message,
            Details = validationErrors,
            ValidationErrors = validationErrors,
            TraceId = context.TraceIdentifier
        });

        await context.Response.WriteAsJsonAsync(response);
    }
}
