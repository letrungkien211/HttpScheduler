using Microsoft.ApplicationInsights.Channel;
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

                    // Success if status code is 4xx
                    if (int.TryParse(request.ResponseCode, out var code))
                    {
                        if (code >= 400 && code < 500)
                        {
                            // If we set the Success property, the SDK won't change it:
                            request.Success = true;
                            // Allow us to filter these requests in the portal:
                            request.Properties["Overridden400s"] = "true";
                        }
                    }
                    break;
                default:
                    break;
            }

            Next.Process(item);
        }
    }
}
