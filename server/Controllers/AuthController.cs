using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Interfaces;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthenticationService authService, UserManager<User> userManager, SignInManager<User> signInManager) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = await signInManager.CheckPasswordSignInAsync(
            user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password" });
            var token = authService.GenerateToken(user);

            return Ok(new { token, user = new { user.Email } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var userExists = await userManager.FindByEmailAsync(request.Email);

            if (userExists != null)
                return BadRequest(new { message = "User already exists" });
            var user = new User
            {
                Email = request.Email,
                UserName = request.Username,
            };
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });
            var token = authService.GenerateToken(user);

            return Ok(new { token, user = new { user.Id, user.Email, user.UserName } });
        }
    }
}