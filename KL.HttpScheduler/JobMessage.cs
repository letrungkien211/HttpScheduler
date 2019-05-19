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
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; set; }

        /// <summary>
        /// Schedule dequeue time in epoch time milliseconds
        /// </summary>
        [JsonProperty("scheduleDequeueTime")]
        [JsonRequired]
        public long ScheduleDequeueTime { get; set; }

        /// <summary>
        /// Enqueued time. Set internally
        /// </summary>
        [JsonProperty("enqueueTime")]
        internal long? EnqueuedTime { get; set; }

        /// <summary>
        /// Callback uri
        /// </summary>
        [JsonRequired]
        public Uri Uri { get; set; }

        /// <summary>
        /// Http method
        /// </summary>
        [JsonProperty("httpMethod")]
        public string HttpMethod { get; set; } = "GET";

        /// <summary>
        /// Content type
        /// </summary>
        [JsonProperty("contentType")]
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Headers
        /// </summary>
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Body
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
