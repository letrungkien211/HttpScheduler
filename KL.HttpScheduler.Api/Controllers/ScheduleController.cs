using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
            actionBlock.Post(httpJob, false);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await sortedSetScheduleClient.GetAsync(id);
            return ret != null ? new JsonResult(ret) : (IActionResult)NotFound();
        }

        [HttpGet("")]
        public async Task<IEnumerable<HttpJob>> List(string id)
        {
            return await sortedSetScheduleClient.ListAsync();
        }

        [HttpPost("batch")]
        public Task<IEnumerable<(bool, Exception)>> ScheduleBatch(
                [FromBody]BatchInput batchInput,
                CancellationToken cancellationToken)
        {
            return sortedSetScheduleClient.ScheduleAsync(batchInput.Jobs, cancellationToken);
        }
    }
}
