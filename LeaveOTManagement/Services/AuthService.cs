using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs;
using LeaveOTManagement.Models.Entities;
using LeaveOTManagement.Service.Interfaces;
using LeaveOTManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LeaveOTManagement.Service
{
    public class AuthService : IAuthService
    {
        private readonly LeaveOTContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            LeaveOTContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            Console.WriteLine("DB Name: " + _context.Database.GetDbConnection().Database);
            Console.WriteLine("Data Source: " + _context.Database.GetDbConnection().DataSource);

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return null;

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == request.Username);

            if (account == null)
                return null;

            if (account.IsLocked == true)
                return null;

            if (account.PasswordHash != request.Password)
                return null;

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == account.UserId);

            if (user == null)
                return null;

            account.LastLoginAt = DateTime.Now;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var roleName = (user.Role?.Name ?? "").Trim();
            var token = GenerateJwtToken(user, roleName);

            return new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = roleName
            };
        }

        public async Task<AuthActionResultDto> SendForgotPasswordOtpAsync(ForgotPasswordSendOtpDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Username or email is required."
                    };
                }

                Console.WriteLine("SEND OTP START");
                Console.WriteLine("Identifier: " + request.Identifier);

                var identifier = request.Identifier.Trim();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == identifier);

                User? user = null;

                if (account != null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);
                }
                else
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier);

                    if (user != null)
                    {
                        account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    }
                }

                if (user == null || account == null)
                {
                    Console.WriteLine("ACCOUNT NOT FOUND");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Account not found."
                    };
                }

                if (account.IsLocked == true)
                {
                    Console.WriteLine("ACCOUNT LOCKED");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "This account is locked."
                    };
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    Console.WriteLine("EMAIL EMPTY");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "This account does not have a valid email address."
                    };
                }

                var oldOtps = await _context.EmailOtps
                    .Where(x => x.UserId == user.Id
                        && x.Purpose == "ForgotPassword"
                        && !x.IsUsed
                        && x.ExpiresAt > DateTime.Now)
                    .ToListAsync();

                if (oldOtps.Any())
                {
                    _context.EmailOtps.RemoveRange(oldOtps);
                    await _context.SaveChangesAsync();
                }

                var otpCode = new Random().Next(100000, 999999).ToString();

                var otp = new EmailOtp
                {
                    UserId = user.Id,
                    Email = user.Email,
                    OtpCode = otpCode,
                    Purpose = "ForgotPassword",
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(5),
                    IsUsed = false
                };

                _context.EmailOtps.Add(otp);
                await _context.SaveChangesAsync();

                Console.WriteLine("User found: " + user.Email);
                Console.WriteLine("OTP: " + otpCode);

                try
                {
                    await _emailService.SendOtpAsync(
                        user.Email,
                        "LeaveOT - Forgot Password OTP",
                        otpCode,
                        "reset password"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SEND EMAIL ERROR: " + ex);
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Failed to send OTP email."
                    };
                }

                return new AuthActionResultDto
                {
                    Success = true,
                    Message = "OTP has been sent to your email."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("SEND OTP SYSTEM ERROR: " + ex);
                return new AuthActionResultDto
                {
                    Success = false,
                    Message = "An unexpected error occurred while sending OTP."
                };
            }
        }

        public async Task<AuthActionResultDto> VerifyForgotPasswordOtpAsync(ForgotPasswordVerifyOtpDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Username or email is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "OTP code is required."
                    };
                }

                Console.WriteLine("VERIFY OTP START");
                Console.WriteLine("Identifier: " + request.Identifier);
                Console.WriteLine("OTP Input: " + request.OtpCode);

                var identifier = request.Identifier.Trim();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == identifier);

                User? user = null;

                if (account != null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);
                }
                else
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier);

                    if (user != null)
                    {
                        account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    }
                }

                if (user == null || account == null)
                {
                    Console.WriteLine("VERIFY: ACCOUNT NOT FOUND");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Account not found."
                    };
                }

                var allOtpRows = await _context.EmailOtps
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                Console.WriteLine("VERIFY: OTP ROW COUNT = " + allOtpRows.Count);

                foreach (var row in allOtpRows)
                {
                    Console.WriteLine(
                        $"OTP ROW => Code={row.OtpCode}, Purpose={row.Purpose}, IsUsed={row.IsUsed}, ExpiresAt={row.ExpiresAt:yyyy-MM-dd HH:mm:ss}, CreatedAt={row.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }

                var otp = await _context.EmailOtps
                    .Where(x => x.UserId == user.Id
                        && x.Purpose == "ForgotPassword"
                        && x.OtpCode == request.OtpCode.Trim()
                        && !x.IsUsed
                        && x.ExpiresAt > DateTime.Now)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    Console.WriteLine("VERIFY: OTP INVALID OR EXPIRED");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "OTP is invalid or expired."
                    };
                }

                var resetToken = new Random().Next(100000, 999999).ToString();

                otp.IsUsed = true;
                await _context.SaveChangesAsync();

                var resetSession = new EmailOtp
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    OtpCode = resetToken,
                    Purpose = "ForgotReset",
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    IsUsed = false
                };

                _context.EmailOtps.Add(resetSession);
                await _context.SaveChangesAsync();

                Console.WriteLine("VERIFY SUCCESS");
                Console.WriteLine("ResetToken: " + resetToken);

                return new AuthActionResultDto
                {
                    Success = true,
                    Message = "OTP verified successfully.",
                    ResetToken = resetToken
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("VERIFY OTP SYSTEM ERROR: " + ex);
                return new AuthActionResultDto
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }

        public async Task<AuthActionResultDto> ResetForgotPasswordAsync(ForgotPasswordResetDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Username or email is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(request.ResetToken))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Reset token is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "New password is required."
                    };
                }

                if (request.NewPassword.Length < 6)
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "New password must be at least 6 characters."
                    };
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Confirm password does not match."
                    };
                }

                Console.WriteLine("RESET PASSWORD START");
                Console.WriteLine("Identifier: " + request.Identifier);
                Console.WriteLine("ResetToken: " + request.ResetToken);

                var identifier = request.Identifier.Trim();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == identifier);

                User? user = null;

                if (account != null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);
                }
                else
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier);

                    if (user != null)
                    {
                        account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    }
                }

                if (user == null || account == null)
                {
                    Console.WriteLine("RESET: ACCOUNT NOT FOUND");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Account not found."
                    };
                }

                var resetSession = await _context.EmailOtps
                    .Where(x => x.UserId == user.Id
                        && x.Purpose == "ForgotReset"
                        && x.OtpCode == request.ResetToken.Trim()
                        && !x.IsUsed
                        && x.ExpiresAt > DateTime.Now)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (resetSession == null)
                {
                    Console.WriteLine("RESET: SESSION INVALID OR EXPIRED");
                    return new AuthActionResultDto
                    {
                        Success = false,
                        Message = "Reset session is invalid or expired."
                    };
                }

                account.PasswordHash = request.NewPassword;
                resetSession.IsUsed = true;

                await _context.SaveChangesAsync();

                Console.WriteLine("RESET SUCCESS");

                return new AuthActionResultDto
                {
                    Success = true,
                    Message = "Password reset successfully."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("RESET PASSWORD SYSTEM ERROR: " + ex);
                return new AuthActionResultDto
                {
                    Success = false,
                    Message = "An unexpected error occurred while resetting password."
                };
            }
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is missing in configuration.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}