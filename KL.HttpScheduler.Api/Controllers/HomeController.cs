using Microsoft.AspNetCore.Mvc;

namespace KL.HttpScheduler.Api.Controllers
{
    [Route("")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
