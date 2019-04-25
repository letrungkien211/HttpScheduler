using Newtonsoft.Json;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Common
{
    /// <summary>
    /// Schedule Input
    /// </summary>
    public class ScheduleInput
    {
        [JsonRequired]
        [JsonProperty("jobs")]
        public List<HttpJob> Jobs { get; set; }
    }
}
