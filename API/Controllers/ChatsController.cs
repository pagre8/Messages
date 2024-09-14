using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {

        [HttpGet("chats/{id}/messages")]
        public IActionResult GetMessages([FromQuery] string cursor, string id)
        {
            if (string.IsNullOrWhiteSpace(cursor)) {  return BadRequest("cursor parameter is required"); }
            if (string.IsNullOrWhiteSpace(id)) { return BadRequest("id parameter is requiered"); }

        }
    }
}
