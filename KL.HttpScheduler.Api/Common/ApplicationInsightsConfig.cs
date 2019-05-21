namespace KL.HttpScheduler.Api.Common
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
        /// Redis connection string
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Api key
        /// </summary>
        public string ApiKey { get; set; }
    }
}
