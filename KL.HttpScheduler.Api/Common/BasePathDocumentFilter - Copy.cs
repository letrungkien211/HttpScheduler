using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Common
{
    /// <summary>
    /// Filter
    /// </summary>
    internal class ApplicationInsightsFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; }
        public ApplicationInsightsFilter(ITelemetryProcessor next)
        {
            Next = next;
        }
        /// <summary>
        /// Process
        /// </summary>
        /// <param name="item"></param>
        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry request)
            {
                if (request.Name == "GET Home/Index")
                {
                    return;
                }
            }

            Next.Process(item);
        }
    }
}
