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
        [JsonRequired]
        [JsonProperty("jobs")]
        [EnsureOneElement]
        public List<HttpJob> Jobs { get; set; }
    }

    public class BatchOutput
    {
        [JsonRequired]
        [JsonProperty("results")]
        public List<ScheduleStatus> Results { get; set; }
    }

    public class ScheduleStatus
    {
        public bool Success { get; set; }
        public Exception Exception { get; set; }
    }
}
