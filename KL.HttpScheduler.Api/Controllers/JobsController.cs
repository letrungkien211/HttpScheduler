using KL.HttpScheduler.Api.Models;
using KL.HttpScheduler.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
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
        private SortedSetScheduleClient SortedSetScheduleClient { get; }
        /// <summary>
        /// Instructor
        /// </summary>
        /// <param name="sortedSetScheduleClient"></param>
        public JobsController(SortedSetScheduleClient sortedSetScheduleClient)
        {
            SortedSetScheduleClient = sortedSetScheduleClient;
        }

        /// <summary>
        /// Schedule a http job
        /// </summary>
        /// <param name="httpJob">http job</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [HttpPost("")]
        [SwaggerRequestExample(typeof(HttpJob), typeof(HttpJobExample))]
        public Task<IActionResult> Schedule([FromBody]HttpJob httpJob)
        {
            return Batch(new BatchInput()
            {
                Jobs = new List<HttpJob>() { httpJob }
            });
        }

        /// <summary>
        /// Cancel an outstanding http job by id. Will return Notfound for jobs that already complete.
        /// </summary>
        /// <param name="id">http job id</param>
        /// <returns></returns>
        /// <response code="404">Not Found</response>    
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                await SortedSetScheduleClient.CancelAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex);
            }
        }

        /// <summary>
        /// Get an outstanding http job by id
        /// </summary>
        /// <param name="id">http job id</param>
        /// <returns></returns>
        /// <response code="404">Not Found</response>    
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HttpJob), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            var ret = await SortedSetScheduleClient.GetAsync(id);
            return ret != null ? Ok(ret) : (IActionResult)NotFound(new KeyNotFoundException($"Id={id} was not found!"));
        }

        /// <summary>
        /// Get all outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<HttpJob>), (int)HttpStatusCode.OK)]
        [SwaggerRequestExample(typeof(GetAllParameters), typeof(GetAllParametersExample))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(HttpJobsExample))]
        public Task<IEnumerable<HttpJob>> GetAll([FromQuery]GetAllParameters parameters)
        {
            return SortedSetScheduleClient.ListAsync(parameters.Start, parameters.Count);
        }

        /// <summary>
        /// Get number of outstanding http jobs
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(long), (int)HttpStatusCode.OK)]
        public Task<long> Count()
        {
            return SortedSetScheduleClient.CountAsync();
        }

        /// <summary>
        /// Schedule a batch of http jobs. All jobs will be dropped if one job is invalid.
        /// </summary>
        /// <param name="batchInput">batch of http jobs</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [HttpPost("[action]")]
        [SwaggerRequestExample(typeof(BatchInput), typeof(BatchInputExample))]
        public async Task<IActionResult> Batch([FromBody]BatchInput batchInput)
        {
            var (success, ex) = await SortedSetScheduleClient.ScheduleAsync(batchInput.Jobs);

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
                    case EnqueueException _:
                        return StatusCode((int)HttpStatusCode.ServiceUnavailable, ex);
                    case ExpiredException _:
                    default:
                        return BadRequest(ex);
                }
            }
        }
    }
}
