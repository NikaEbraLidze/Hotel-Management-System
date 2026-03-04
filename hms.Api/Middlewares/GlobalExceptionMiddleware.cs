using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace hms.Api.Middlewares
{
    public class GlobalExceptionMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            throw new System.NotImplementedException();
        }
    }
}