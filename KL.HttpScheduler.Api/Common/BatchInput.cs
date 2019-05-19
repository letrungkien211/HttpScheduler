using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Common
{
    /// <summary>
    /// Schedule Input
    /// </summary>
    public class BatchInput
    {
        /// <summary>
        /// Jobs
        /// </summary>
        [JsonRequired]
        [JsonProperty("jobs")]
        [EnsureOneElement]
        public List<HttpJob> Jobs { get; set; }
    }

    /// <summary>
    /// Batch output
    /// </summary>
    public class BatchOutput
    {
        /// <summary>
        /// Results
        /// </summary>
        [JsonRequired]
        [JsonProperty("results")]
        public List<ScheduleStatus> Results { get; set; }
    }

    /// <summary>
    /// Schedule status
    /// </summary>
    public class ScheduleStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}
