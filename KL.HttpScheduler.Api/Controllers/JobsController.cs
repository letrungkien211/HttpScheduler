using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Controllers
{
    /// <summary>
    /// Jobs scheduler
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly SortedSetScheduleClient sortedSetScheduleClient;
        /// <summary>
        /// Instructor
        /// </summary>
        /// <param name="sortedSetScheduleClient"></param>
        public JobsController(SortedSetScheduleClient sortedSetScheduleClient)
        {
            this.sortedSetScheduleClient = sortedSetScheduleClient;
        }

        /// <summary>
        /// Schedule a http job
        /// </summary>
        /// <param name="httpJob">Http Job</param>
        /// <returns></returns>
        [HttpPost("")]
        [SwaggerRequestExample(typeof(HttpJob), typeof(HttpJobExample))]
        [ProducesResponseType(typeof(Exception), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Schedule([FromBody]HttpJob httpJob)
        {
            var (success, ex) = (await sortedSetScheduleClient.ScheduleAsync(new[] { httpJob })).First();
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Cancel a http job by id
        /// </summary>
        /// <param name="id">httpjob id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                await sortedSetScheduleClient.CancelAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get an outstanding http job by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await sortedSetScheduleClient.GetAsync(id);
            return ret != null ? new JsonResult(ret) : (IActionResult)NotFound($"Id={id} was not found!");
        }

        /// <summary>
        /// Get all outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<HttpJob>> GetAll()
        {
            return await sortedSetScheduleClient.ListAsync();
        }

        /// <summary>
        /// Get number of outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("Count")]
        public Task<long> Count()
        {
            return sortedSetScheduleClient.CountAsync();
        }

        /// <summary>
        /// Schedule batch
        /// </summary>
        /// <param name="batchInput"></param>
        /// <returns></returns>
        [HttpPost("Batch")]
        public async Task<BatchOutput> ScheduleBatch([FromBody]BatchInput batchInput)
        {
            var rets = await sortedSetScheduleClient.ScheduleAsync(batchInput.Jobs);

            return new BatchOutput()
            {
                Results = rets.Select(x => new ScheduleStatus()
                {
                    Success = x.Item1,
                    Exception = x.Item2
                }).ToList()
            };
        }
    }
}
