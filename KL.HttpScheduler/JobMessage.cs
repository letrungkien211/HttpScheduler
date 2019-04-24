using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Job Message
    /// </summary>
    public class HttpJob
    {
        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; set; }

        [JsonProperty("isCancellable")]
        public bool IsCancellable { get; set; } = false;

        [JsonProperty("scheduleDequeueTime")]
        [JsonRequired]
        public long ScheduleDequeueTime { get; set; }

        [JsonProperty("enqueueTime")]
        public long? EnqueuedTime { get; internal set; }

        [JsonRequired]
        public Uri Uri { get; set; }

        [JsonProperty("httpMethod")]
        public string HttpMethod { get; set; } = "POST";


        [JsonProperty("contentType")]
        public string ContentType { get; set; } = "application/json";

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
