﻿using KL.HttpScheduler.Api.Common;
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
    [Produces("application/json")]
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
        [ProducesResponseType(typeof(ConflictException), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(Exception), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Schedule([FromBody]HttpJob httpJob)
        {
            var (success, ex) = (await sortedSetScheduleClient.ScheduleAsync(new[] { httpJob })).First();
            if (success)
            {
                return Ok();
            }
            else
            {
                switch (ex)
                {
                    case ConflictException _:
                        return StatusCode((int)HttpStatusCode.Conflict, ex);
                    default:
                        return BadRequest(ex);
                }
            }
        }

        /// <summary>
        /// Cancel an outstanding http job by id. Will return Notfound for jobs that already complete.
        /// </summary>
        /// <param name="id">httpjob id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType(typeof(HttpJob), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await sortedSetScheduleClient.GetAsync(id);
            return ret != null ? Ok(ret) : (IActionResult)NotFound($"Id={id} was not found!");
        }

        /// <summary>
        /// Get all outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<HttpJob>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<HttpJob>> GetAll()
        {
            return await sortedSetScheduleClient.ListAsync();
        }

        /// <summary>
        /// Get number of outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(long), (int)HttpStatusCode.OK)]
        public Task<long> Count()
        {
            return sortedSetScheduleClient.CountAsync();
        }

        /// <summary>
        /// Schedule batch
        /// </summary>
        /// <param name="batchInput"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(BatchOutput), (int)HttpStatusCode.OK)]
        public async Task<BatchOutput> Batch([FromBody]BatchInput batchInput)
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
