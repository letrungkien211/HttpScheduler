﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        /// Get all logs related to this id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hoursAgo"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery]int hoursAgo = 24)
        {
            var client = HttpClientFactory.CreateAppInsightsClient();
            var query = $"traces | where timestamp > ago({hoursAgo}h) | where customDimensions.id=='{id}'| project timestamp , message ";
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