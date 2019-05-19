﻿using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly SortedSetScheduleClient sortedSetScheduleClient;
        public JobsController(SortedSetScheduleClient sortedSetScheduleClient)
        {
            this.sortedSetScheduleClient = sortedSetScheduleClient;
        }

        [HttpPost("")]
        public async Task<IActionResult> Schedule(
            [FromBody]HttpJob httpJob,
            CancellationToken cancellationToken)
        {
            var (success, ex) = (await sortedSetScheduleClient.ScheduleAsync(new[] { httpJob }, cancellationToken)).First();
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(ex);
            }
        }

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

        [HttpPost("[action]")]
        public IActionResult Execute([FromBody]HttpJob httpJob, [FromServices]MyActionBlock actionBlock)
        {
            return actionBlock.Post(httpJob) ? Ok() : (IActionResult)StatusCode((int)HttpStatusCode.ServiceUnavailable, "Queue is full");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await sortedSetScheduleClient.GetAsync(id);
            return ret != null ? new JsonResult(ret) : (IActionResult)NotFound();
        }

        [HttpGet("")]
        public async Task<IEnumerable<HttpJob>> GetAll()
        {
            return await sortedSetScheduleClient.ListAsync();
        }

        [HttpPost("Batch")]
        public async Task<BatchOutput> ScheduleBatch(
                [FromBody]BatchInput batchInput,
                CancellationToken cancellationToken)
        {
            var rets = await sortedSetScheduleClient.ScheduleAsync(batchInput.Jobs, cancellationToken);
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
