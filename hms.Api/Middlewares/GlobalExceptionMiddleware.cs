using System;
using System.Net;
using System.Threading.Tasks;
using hms.Api;
using hms.Application.Models.Exceptions;
using Microsoft.AspNetCore.Http;

namespace hms.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
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
    }
}
