using LeaveOTManagement.DTOs;
using LeaveOTManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeaveOTManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized("Sai email hoặc password");

            return Ok(result);
        }
    }
}