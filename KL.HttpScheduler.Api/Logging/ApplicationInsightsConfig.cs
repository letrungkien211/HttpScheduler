using System;

namespace KL.HttpScheduler.Api.Logging
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class ApplicationInsightsConfig
    {
        /// <summary>
        /// Is Valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ApplicationId) && !string.IsNullOrEmpty(ApiKey);
        }

        /// <summary>
        /// Application Id
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Instrumentation key
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Api key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Api Url
        /// </summary>
        public Uri ApiUrl() => new Uri($"https://api.applicationinsights.io/v1/apps/{ApplicationId}/");
    }
}
