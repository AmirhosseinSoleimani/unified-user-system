
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await next(context);
                return;
            }
            try
            {
                await next(context);

            } catch (DomainException ex)
            {
                await WriteProblem(context, StatusCodes.Status400BadRequest, "Domain error", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteProblem(context, StatusCodes.Status409Conflict, "Conflic", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteProblem(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
            }
            catch (ArgumentException ex)
            {
                await WriteProblem(context, StatusCodes.Status400BadRequest, "Bad request", ex.Message);
            }
            catch (Exception ex)
            {
                await WriteProblem(context, StatusCodes.Status500InternalServerError, "server error", $"Unexpected error: {ex.Message}");
            }
        }

        private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
