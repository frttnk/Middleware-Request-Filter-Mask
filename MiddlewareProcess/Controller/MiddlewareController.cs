using Microsoft.AspNetCore.Mvc;

namespace MiddlewareExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MiddlewareController : ControllerBase
    {
        [HttpPost]
        public IActionResult ReturnResponse([FromBody] Dictionary<string, object> requestData)
        {
            return Ok(new
            {
                Data = requestData
            });
        }
    }
}