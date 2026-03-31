using System.Security.Claims;
using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs;
using LeaveOTManagement.Models.Entities;
using LeaveOTManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly LeaveOTContext _context;
        private readonly IEmailService _emailService;

        public ProfileController(LeaveOTContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            return Ok(new
            {
                id = user.Id,
                employeeCode = user.EmployeeCode,
                fullName = user.FullName,
                email = user.Email,
                username = account?.Username ?? "",
                role = user.Role?.Name ?? "",
                departmentId = user.DepartmentId,
                departmentName = user.Department?.Name ?? "",
                managerId = user.ManagerId,
                managerCode = user.Manager?.EmployeeCode,
                managerName = user.Manager?.FullName,
                managerEmail = user.Manager?.Email,
                isActive = user.IsActive ?? false,
                createdAt = user.CreatedAt,
                lastLoginAt = account?.LastLoginAt
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new { message = "Full name is required." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            var normalizedEmail = dto.Email.Trim();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.Id != userId);

            if (emailExists)
                return BadRequest(new { message = "Email already exists." });

            user.FullName = dto.FullName.Trim();
            user.Email = normalizedEmail;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                fullName = user.FullName,
                email = user.Email
            });
        }

        [HttpPost("send-change-password-otp")]
        public async Task<IActionResult> SendChangePasswordOtp([FromBody] SendChangePasswordOtpDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return BadRequest(new { message = "Current password is required." });

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "New password is required." });

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "Confirm password does not match." });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { message = "New password must be at least 6 characters." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Theo project hiện tại: đang so sánh plain text
            if (account.PasswordHash != dto.CurrentPassword)
                return BadRequest(new { message = "Current password is incorrect." });

            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest(new { message = "New password must be different from current password." });

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { message = "Your account does not have an email address." });

            var oldActiveOtps = await _context.EmailOtps
                .Where(x => x.UserId == userId
                    && x.Purpose == "ChangePassword"
                    && !x.IsUsed
                    && x.ExpiresAt > DateTime.Now)
                .ToListAsync();

            if (oldActiveOtps.Any())
            {
                _context.EmailOtps.RemoveRange(oldActiveOtps);
                await _context.SaveChangesAsync();
            }

            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new EmailOtp
            {
                UserId = userId,
                Email = user.Email,
                OtpCode = otpCode,
                Purpose = "ChangePassword",
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(
                user.Email,
                "LeaveOT - Change Password OTP",
                otpCode,
                "change password"
            );

            return Ok(new { message = "OTP has been sent to your email." });
        }

        [HttpPost("confirm-change-password")]
        public async Task<IActionResult> ConfirmChangePassword([FromBody] ConfirmChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "Confirm password does not match." });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            if (account.PasswordHash != dto.CurrentPassword)
                return BadRequest(new { message = "Current password is incorrect." });

            var otp = await _context.EmailOtps
                .Where(x => x.UserId == userId
                    && x.Purpose == "ChangePassword"
                    && x.OtpCode == dto.OtpCode
                    && !x.IsUsed
                    && x.ExpiresAt > DateTime.Now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return BadRequest(new { message = "OTP is invalid or expired." });

            account.PasswordHash = dto.NewPassword;
            otp.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }
}