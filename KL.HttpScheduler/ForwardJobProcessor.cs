using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Forward this job to another server
    /// </summary>
    public class ForwardJobProcessor : IJobProcessor
    {
        public const string ForwardClientName = "Forward";
        private IHttpClientFactory HttpClientFactory { get; }
        public ForwardJobProcessor(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken)
        {
            var client = HttpClientFactory.CreateClient(ForwardClientName);

            var req = new HttpRequestMessage(new HttpMethod(httpJob.HttpMethod), "")
            {
                Content = new StringContent(JsonConvert.SerializeObject(httpJob), Encoding.UTF8, "application/json")
            };

            return client.SendAsync(req, cancellationToken);
        }
    }
}
