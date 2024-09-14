using API.Data_access;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendMessageController : Controller
    {

        private readonly ApiDbContext _dbContext;
        public SendMessageController(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<ActionResult<HttpStatusCode>> SendMessage(string text, int from, int to)
        {
            Messages message = new Messages()
            {
                Text = text,
                Timestamp = DateTime.Now,
                From = new UserData() { Id = from },
                To = new UserData() { Id = to }
            };

            _dbContext.Messages.AddAsync(message);
            _dbContext.SaveChangesAsync();

            return Ok(message);
        }
    }
}
