﻿using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Examples;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// Schedule http jobs
        /// </summary>
        /// <param name="httpJob">Http Job</param>
        /// <returns></returns>
        [HttpPost("")]
        [SwaggerRequestExample(typeof(HttpJob), typeof(HttpJobExample))]
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
        /// Cancel http jobs
        /// </summary>
        /// <param name="id">httpjob id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
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
        /// Get oustanding job by id. Execuated job cannot be get anymore
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await sortedSetScheduleClient.GetAsync(id);
            return ret != null ? new JsonResult(ret) : (IActionResult)NotFound();
        }

        /// <summary>
        /// Get all oustanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<HttpJob>> GetAll()
        {
            return await sortedSetScheduleClient.ListAsync();
        }

        /// <summary>
        /// Get number of outstanding jobs
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
