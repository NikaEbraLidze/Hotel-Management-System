using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace hms.Api.Filters
{
    public sealed class RequestLoggingActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<RequestLoggingActionFilter> _logger;

        public RequestLoggingActionFilter(ILogger<RequestLoggingActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerName = actionDescriptor?.ControllerName ?? "UnknownController";
            var actionName = actionDescriptor?.ActionName ?? "UnknownAction";
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var traceId = httpContext.TraceIdentifier;

            _logger.LogInformation(
                "Handling {Method} {Path} in {Controller}.{Action}. TraceId: {TraceId}, UserId: {UserId}",
                request.Method,
                request.Path,
                controllerName,
                actionName,
                traceId,
                userId);

            var stopwatch = Stopwatch.StartNew();
            var executedContext = await next();
            stopwatch.Stop();

            if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
            {
                return;
            }

            var statusCode = httpContext.Response.StatusCode;
            const string message =
                "Completed {Method} {Path} in {Controller}.{Action} with {StatusCode} in {ElapsedMs} ms. TraceId: {TraceId}, UserId: {UserId}";

            if (statusCode >= 500)
            {
                _logger.LogError(
                    message,
                    request.Method,
                    request.Path,
                    controllerName,
                    actionName,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    traceId,
                    userId);
                return;
            }

            if (statusCode >= 400)
            {
                _logger.LogWarning(
                    message,
                    request.Method,
                    request.Path,
                    controllerName,
                    actionName,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    traceId,
                    userId);
                return;
            }

            _logger.LogInformation(
                message,
                request.Method,
                request.Path,
                controllerName,
                actionName,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                traceId,
                userId);
        }
    }
}
