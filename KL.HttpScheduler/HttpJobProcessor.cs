using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Job processor
    /// </summary>
    public class HttpJobProcessor : IJobProcessor
    {
        private IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public HttpJobProcessor(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpStatusCode> ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken)
        {
            var client = HttpClientFactory.CreateClient();

            var req = new HttpRequestMessage(new HttpMethod(httpJob.HttpMethod), httpJob.Uri);

            if (httpJob.Body != null)
            {
                req.Content = string.IsNullOrEmpty(httpJob.ContentType) ?
                    new StringContent(httpJob.Body, Encoding.UTF8) :
                    new StringContent(httpJob.Body, Encoding.UTF8, httpJob.ContentType);
            }

            if (httpJob.Headers != null)
            {
                foreach(var kv in httpJob.Headers)
                {
                    req.Headers.Add(kv.Key, kv.Value);
                }
            }

            return (await client.SendAsync(req, cancellationToken).ConfigureAwait(false)).StatusCode;
        }
    }
}
