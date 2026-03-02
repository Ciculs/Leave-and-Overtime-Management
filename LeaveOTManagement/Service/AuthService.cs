using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs;
using LeaveOTManagement.Models.Entities;
using LeaveOTManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public AuthService(LeaveOTContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Debug xem đang connect DB nào
            Console.WriteLine("DB Name: " + _context.Database.GetDbConnection().Database);
            Console.WriteLine("Data Source: " + _context.Database.GetDbConnection().DataSource);

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return null;

            var account = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Username == request.Username);

            if (account == null)
                return null;

            if (account.IsLocked == true)
                return null;

            // So sánh password (plain text theo DB hiện tại)
            if (account.PasswordHash != request.Password)
                return null;

            // Lấy user riêng để tránh Include lỗi mapping
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == account.UserId);

            if (user == null)
                return null;

            // Update LastLoginAt riêng
            account.LastLoginAt = DateTime.Now;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? ""
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireMinutes = Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]);

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is missing in configuration.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}