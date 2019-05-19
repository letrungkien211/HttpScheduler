using Microsoft.AspNetCore.Mvc;

namespace KL.HttpScheduler.Api.Controllers
{
    /// <summary>
    /// Home controller
    /// </summary>
    [Route("")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
