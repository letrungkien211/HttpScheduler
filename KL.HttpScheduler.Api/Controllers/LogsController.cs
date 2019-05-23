using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Controllers
{
    /// <summary>
    /// Logs controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class LogsController : ControllerBase
    {
        private IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public LogsController(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// Get all logs related to this id. Logs index latency is about 5 minutes 
        /// (so you need to wait up to 5 minutes after scheduling a job to see its log to appear)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hoursAgo"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id, [FromQuery]int hoursAgo = 24)
        {
            var client = HttpClientFactory.CreateAppInsightsClient();
            var query = $"traces | where timestamp > ago({hoursAgo}h) | where customDimensions.operation_Id=='{id}'| project timestamp , message ";
            var req = new HttpRequestMessage(HttpMethod.Post, "query")
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { query }), Encoding.UTF8, "application/json")
            };

            var res = await client.SendAsync(req);

            var contentResult = Content(await res.Content.ReadAsStringAsync(), "application/json");
            contentResult.StatusCode = (int)res.StatusCode;
            return contentResult;
        }
    }
}
