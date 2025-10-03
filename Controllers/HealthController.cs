using Microsoft.AspNetCore.Mvc;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET: api/health
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.Now });
        }
    }
}
