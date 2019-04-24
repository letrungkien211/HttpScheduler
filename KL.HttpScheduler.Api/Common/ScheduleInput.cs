using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Common
{
    public class ScheduleInput
    {
        [JsonRequired]
        [JsonProperty("jobs")]
        public List<HttpJob> Jobs { get; set; }
    }
}
