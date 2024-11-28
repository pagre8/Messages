using API_Server.Interfaces;
using API_Server.Models;
using API_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Server.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IUserService _userService) : ControllerBase
    {
        readonly IUserService userService = _userService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterRequest request)
        {
            if (!ModelState.IsValid) {
                return BadRequest(new { error = "invalid input" });
            }

            if (request.Username.Length == 0 || request.Password.Length == 0)
            {
                return BadRequest(new {error= "Username and password are required." });
            }

            if(await userService.CheckIfUserExistFromUsername(request.Username))
            {
                return Conflict(new { error = "Username is already taken." });
            }

            if (await userService.CheckIfUserExistFromEmail(request.Email))
            {
                return Conflict(new { error = "User with this email already exists." });
            }
    
            if(!await userService.RegisterUser(request))
            {
                return Problem("User registration failed due to an internal error. Please try again later.", statusCode: 500);
            }

            return Created(); // return key or something
        }
    }
}
