﻿using Newtonsoft.Json;
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
        /// Positive: Absolute Value
        /// Negative: Relative to current time. E.g: -1000 means run after 1s
        /// Zero: Execute immediately
        /// </summary>
        [JsonProperty("scheduleDequeueTime")]
        [JsonRequired]
        public long ScheduleDequeueTime { get; set; }

        /// <summary>
        /// Actual dequeued time. Use internally. Do not set this value via API (will be ignored).
        /// </summary>
        [JsonProperty("dequeuedTime")]
        internal long? DequeuedTime { get; set; }

        /// <summary>
        /// When actual dequeue time is late, job will be discarded. Default value is 5000 milliseconds.
        /// </summary>
        [JsonProperty("scheduleDequeueTimeLatencyTimeout")]
        public long ScheduleDequeueTimeLatencyTimeout { get; set; } = (long)TimeSpan.FromSeconds(5).TotalMilliseconds;

        /// <summary>
        /// Enqueued time. Use internally. Do not set this value via API (will be ignored).
        /// </summary>
        [JsonProperty("enqueuedTime")]
        internal long? EnqueuedTime { get; set; }

        /// <summary>
        /// Callback uri
        /// </summary>
        [JsonRequired]
        public Uri Uri { get; set; }

        /// <summary>
        /// Http method. Default Value = "GET"
        /// </summary>
        [JsonProperty("httpMethod")]
        public string HttpMethod { get; set; } = "GET";

        /// <summary>
        /// Content type. Default Value = "application/json"
        /// </summary>
        [JsonProperty("contentType")]
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Headers. Default Value = null
        /// </summary>
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Body. Default Value = null
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Batch Id. For internal use. Do not set this value.
        /// </summary>
        [JsonProperty("batchId")]
        internal string BatchId { get; set; }
    }
}
