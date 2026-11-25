using Microsoft.AspNetCore.Mvc;
using Pennywise.BLL.Exceptions;

namespace Pennywise.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException e => (StatusCodes.Status404NotFound, e.Message),
                ConflictException e => (StatusCodes.Status409Conflict, e.Message),
                BusinessRuleException e => (StatusCodes.Status400BadRequest, e.Message),
                UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized, e.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title });
        }
    }
}
