using API.Data_access;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class testController : Controller
    {
        private readonly ApiDbContext _dbContext;
        public testController(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserData>>> GetUsers()
        {
            return _dbContext.User_data.ToList();
        }
    }
}
