﻿using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

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
            switch (item)
            {
                case RequestTelemetry request:
                    if (request.Name == "GET Home/Index")
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }


            Next.Process(item);
        }
    }
}
