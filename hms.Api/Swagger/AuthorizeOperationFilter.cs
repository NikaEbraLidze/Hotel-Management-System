using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace hms.Api.Swagger
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
            var controllerAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();

            var hasAllowAnonymous = methodAttributes.OfType<AllowAnonymousAttribute>().Any()
                || controllerAttributes.OfType<AllowAnonymousAttribute>().Any();

            if (hasAllowAnonymous)
            {
                return;
            }

            var hasAuthorize = methodAttributes.OfType<AuthorizeAttribute>().Any()
                || controllerAttributes.OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize)
            {
                return;
            }

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            });

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }
    }
}
