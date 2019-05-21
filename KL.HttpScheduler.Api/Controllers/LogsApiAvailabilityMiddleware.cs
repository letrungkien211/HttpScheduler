using System.Net;
using System.Threading.Tasks;
using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace KL.HttpScheduler.Api.Controllers
{
    /// <summary>
    /// Check if logs api is available or not
    /// </summary>
    internal class LogsApiAvailabilityMiddleware
    {
        private RequestDelegate Next { get; }
        private ApplicationInsightsConfig ApplicationInsightsConfig { get; }

        public LogsApiAvailabilityMiddleware(RequestDelegate next, ApplicationInsightsConfig applicationInsightsConfig)
        {
            Next = next;
            ApplicationInsightsConfig = applicationInsightsConfig;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api/jobs") && !ApplicationInsightsConfig.IsValid())
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                await httpContext.Response.WriteAsync("Application Insights APIs are not configured.");
                return;
            }
            await Next(httpContext);
        }
    }

    internal static class LogsApiAvailabilityMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogsApiAvailabilityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogsApiAvailabilityMiddleware>();
        }
    }
}
