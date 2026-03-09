using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using hms.Api;
using hms.Application.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace hms.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogException(HttpContext context, Exception ex)
        {
            var statusCode = GetStatusCode(ex);
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            const string message =
                "Request {Method} {Path} failed with {StatusCode}. TraceId: {TraceId}, UserId: {UserId}";

            switch (ex)
            {
                case ArgumentException:
                case BadRequestException:
                case UnauthorizedAccessException:
                case ForbiddenException:
                case NotFoundException:
                case ConflictException:
                case IdentityOperationException:
                    _logger.LogWarning(
                        ex,
                        message,
                        context.Request.Method,
                        context.Request.Path,
                        (int)statusCode,
                        context.TraceIdentifier,
                        userId);
                    break;
                default:
                    _logger.LogError(
                        ex,
                        message,
                        context.Request.Method,
                        context.Request.Path,
                        (int)statusCode,
                        context.TraceIdentifier,
                        userId);
                    break;
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var apiResponse = ex switch
            {
                ArgumentException argumentException => CommonResponse.Fail(
                    new[] { argumentException.Message },
                    argumentException.Message,
                    HttpStatusCode.BadRequest),
                BadRequestException badRequestException => CommonResponse.Fail(
                    new[] { badRequestException.Message },
                    badRequestException.Message,
                    HttpStatusCode.BadRequest),
                UnauthorizedAccessException unauthorizedAccessException => CommonResponse.Fail(
                    new[] { unauthorizedAccessException.Message },
                    unauthorizedAccessException.Message,
                    HttpStatusCode.Unauthorized),
                ForbiddenException forbiddenException => CommonResponse.Fail(
                    new[] { forbiddenException.Message },
                    forbiddenException.Message,
                    HttpStatusCode.Forbidden),
                NotFoundException notFoundException => CommonResponse.Fail(
                    new[] { notFoundException.Message },
                    notFoundException.Message,
                    HttpStatusCode.NotFound),
                ConflictException conflictException => CommonResponse.Fail(
                    new[] { conflictException.Message },
                    conflictException.Message,
                    HttpStatusCode.Conflict),
                IdentityOperationException identityOperationException => CommonResponse.Fail(
                    identityOperationException.Errors,
                    identityOperationException.Message,
                    HttpStatusCode.BadRequest),
                _ => CommonResponse.Fail(
                    Array.Empty<string>(),
                    "An unexpected error occurred.",
                    HttpStatusCode.InternalServerError)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = Convert.ToInt32(apiResponse.StatusCode);

            return context.Response.WriteAsJsonAsync(apiResponse);
        }

        private static HttpStatusCode GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                BadRequestException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ForbiddenException => HttpStatusCode.Forbidden,
                NotFoundException => HttpStatusCode.NotFound,
                ConflictException => HttpStatusCode.Conflict,
                IdentityOperationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}
