using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterviewAssignment3.Middleware
{
    public static class CallerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCaller(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CallerMiddleware>();
        }
    }
}

