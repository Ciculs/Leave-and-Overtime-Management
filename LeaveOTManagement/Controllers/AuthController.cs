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
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }

        [HttpPost("forgot-password/send-otp")]
        public async Task<IActionResult> SendForgotPasswordOtp([FromBody] ForgotPasswordSendOtpDto request)
        {
            var result = await _authService.SendForgotPasswordOtpAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("forgot-password/verify-otp")]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] ForgotPasswordVerifyOtpDto request)
        {
            var result = await _authService.VerifyForgotPasswordOtpAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                resetToken = result.ResetToken
            });
        }

        [HttpPost("forgot-password/reset")]
        public async Task<IActionResult> ResetForgotPassword([FromBody] ForgotPasswordResetDto request)
        {
            var result = await _authService.ResetForgotPasswordAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}