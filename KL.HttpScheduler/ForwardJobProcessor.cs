using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Forward this job to another server
    /// </summary>
    public class ForwardJob
    {
        public const string ForwardClientName = "Forward";
        private IHttpClientFactory HttpClientFactory { get; }
        private TelemetryClient TelemetryClient { get; }
        public ForwardJob(IHttpClientFactory httpClientFactory, TelemetryClient telemetryClient)
        {
            HttpClientFactory = httpClientFactory;
            TelemetryClient = telemetryClient;
        }

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ForwardAsync(HttpJob httpJob)
        {
            try
            {
                var client = HttpClientFactory.CreateClient(ForwardClientName);

                var req = new HttpRequestMessage(new HttpMethod(httpJob.HttpMethod), "")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(httpJob), Encoding.UTF8, "application/json")
                };

                using (var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    await client.SendAsync(req, cancelSource.Token);
                }
            }
            catch(Exception ex)
            {
                TelemetryClient.TrackException(ex, new Dictionary<string, string>()
                {
                    ["Id"] = httpJob.Id,
                    ["HttpJob"] = JsonConvert.SerializeObject(httpJob)
                });
            }
        }
    }
}
