using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using InterviewAssignment3.Common.Objects;

namespace InterviewAssignment3.Middleware
{
    public class CallerMiddleware
    {
        private readonly RequestDelegate _next;

        private static string USER_ID_HEADER = "UserId";

        public CallerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Caller caller)
        {
            bool isForwardedIpAddressAllowed = true;
            string userId = String.IsNullOrWhiteSpace(context.User?.Identity?.Name) ? "Anonymous" : context.User.Identity.Name;

            // Check if there is a UserId send in the header.
            if (context.Request?.Headers?.ContainsKey(USER_ID_HEADER) ?? false)
            {
                userId = context.Request?.Headers?[USER_ID_HEADER] ?? "";
                userId = userId.Trim();
            }

            caller.UserId = userId;

            caller.UserAgent = context.Request?.Headers?["User-Agent"].FirstOrDefault() ?? String.Empty;

            if (isForwardedIpAddressAllowed)
            {
                string forwardedIpAddressHeaderValue = context.Request?.Headers?["X-Forwarded-For"].FirstOrDefault() ?? String.Empty;
                IEnumerable<string> forwardedIpAddresses = forwardedIpAddressHeaderValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(obj => obj.Trim());

                if (forwardedIpAddresses.Any())
                {
                    caller.FromRemoteIpAddress = forwardedIpAddresses.First();
                }
                else
                {
                    caller.FromRemoteIpAddress = context.Connection?.RemoteIpAddress?.ToString() ?? String.Empty;
                }
            }
            else
            {
                caller.FromRemoteIpAddress = context.Connection?.RemoteIpAddress?.ToString() ?? String.Empty;
            }

            // Call the next delegate/middleware in the pipeline
            await this._next(context);
        }
    }
}
