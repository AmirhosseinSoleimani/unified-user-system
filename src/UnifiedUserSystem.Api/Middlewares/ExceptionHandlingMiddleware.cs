using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILocalizedMessageCache _localizedMessageCache;

        public ExceptionHandlingMiddleware(ILocalizedMessageCache localizedMessageCache)
        {
            _localizedMessageCache = localizedMessageCache;
        }

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
                await WriteProblem(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Invalid Information",
                    new LocalizedMessage("Invalid Information", "خطا در اطلاعات واردشده"),
                    new LocalizedMessage(ex.Message, ".اطلاعات واردشده با شرایط سرویس مطابقت ندارد"));
            }
            catch (InvalidOperationException ex)
            {
                await WriteProblem(
                    context,
                    StatusCodes.Status409Conflict,
                    "conflict",
                    new LocalizedMessage("Request Cannot Be Completed", "امکان انجام درخواست وجود ندارد"),
                    new LocalizedMessage(ex.Message, ".این درخواست در وضعیت فعلی قابل انجام نیست. لطفاً کمی بعد دوباره تلاش کنید"));
            }
            catch (KeyNotFoundException ex)
            {
                await WriteProblem(
                    context,
                    StatusCodes.Status404NotFound,
                    "not_found",
                    new LocalizedMessage("Not found", "یافت نشد"),
                    new LocalizedMessage(ex.Message, "منبع مورد نظر پیدا نشد."));
            }
            catch (ArgumentException ex)
            {
                await WriteProblem(
                    context,
                    StatusCodes.Status400BadRequest,
                    "bad_request",
                    new LocalizedMessage("Invalid Request", "درخواست نامعتبر"),
                    new LocalizedMessage(ex.Message, ".درخواست ارسال‌شده معتبر نیست. لطفاً اطلاعات را بررسی کرده و دوباره تلاش کنید"));
            }
            catch (Exception)
            {
                await WriteProblem(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "server_error",
                    new LocalizedMessage("Server error", "خطای سرور"),
                    new LocalizedMessage("An unexpected error occurred. Please try again later.", ".خطایی غیرمنتظره رخ داد. لطفاً کمی بعد دوباره تلاش کنید"));
            }
        }

        private async Task WriteProblem(
            HttpContext context,
            int status,
            string errorKey,
            LocalizedMessage fallbackTitle,
            LocalizedMessage fallbackDetail
            )
        {
            var language = ResolveLanguage(context);
            var title = await _localizedMessageCache.GetAsync(
                $"{errorKey}.title",
                fallbackTitle,
                context.RequestAborted);
            var detail = await _localizedMessageCache.GetAsync(
                $"{errorKey}.detail",
                fallbackDetail,
                context.RequestAborted);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title.Get(language),
                Detail = detail.Get(language)
            };
            problem.Extensions["errorCode"] = errorKey;
            problem.Extensions["language"] = language;
            problem.Extensions["localized"] = new
            {
                title = new { en = title.English, fa = title.Persian },
                detail = new { en = detail.English, fa = detail.Persian }
            };

            await context.Response.WriteAsJsonAsync(problem);
        }

        private static string ResolveLanguage(HttpContext context)
        {
            var language = context.Request.Headers["X-Language"].FirstOrDefault()
                ?? context.Request.Headers["Accept-Language"].FirstOrDefault()
                ?? context.Request.Query["language"].FirstOrDefault();

            return language?.StartsWith("fa", StringComparison.OrdinalIgnoreCase) == true
                ? "fa"
                : "en";
        }
    }
}
