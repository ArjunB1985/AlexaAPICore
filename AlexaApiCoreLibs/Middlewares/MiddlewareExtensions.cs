using AlexaApiCoreLibs.Validators;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaApiCoreLibs.Middlewares
{
   
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestValidatorMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestValidatorMiddleware>();
        }
    }
}
