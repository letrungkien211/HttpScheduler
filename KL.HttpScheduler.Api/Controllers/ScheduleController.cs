using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace KL.HttpScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly SortedSetScheduleClient sortedSetScheduleClient;
        public JobsController(SortedSetScheduleClient sortedSetScheduleClient)
        {
            this.sortedSetScheduleClient = sortedSetScheduleClient;
        }
        [HttpPost("")]
        public Task Schedule(
            [FromBody]ScheduleInput scheduleInput,
            CancellationToken cancellationToken)
        {
            return sortedSetScheduleClient.ScheduleAsync(scheduleInput.Jobs, cancellationToken);
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
    }
}
