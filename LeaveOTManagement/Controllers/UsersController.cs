using LeaveOTManagement.Data;
using LeaveOTManagement.Models.DTOs;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public UsersController(LeaveOTContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL USERS
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Include(u => u.Manager)
                .Select(u => new
                {
                    u.Id,
                    u.EmployeeCode,
                    u.FullName,
                    u.Email,
                    Department = u.Department.Name,
                    Role = u.Role.Name,
                    Manager = u.Manager != null ? u.Manager.FullName : null,
                    u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        // ===============================
        // GET MANAGERS
        // ===============================
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Manager")
                .Select(u => new
                {
                    u.Id,
                    u.FullName
                })
                .ToListAsync();

            return Ok(managers);
        }

        // ===============================
        // GET DEPARTMENTS
        // ===============================
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name
                })
                .ToListAsync();

            return Ok(departments);
        }

        // ===============================
        // GET ROLES
        // ===============================
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .ToListAsync();

            return Ok(roles);
        }

        // ===============================
        // CREATE USER
        // ===============================
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lastUser = await _context.Users
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            string newCode = "EMP001";

            if (lastUser != null && !string.IsNullOrEmpty(lastUser.EmployeeCode))
            {
                int number = int.Parse(lastUser.EmployeeCode.Substring(3));
                newCode = "EMP" + (number + 1).ToString("D3");
            }

            var user = new User
            {
                EmployeeCode = newCode,
                FullName = dto.FullName,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,
                RoleId = dto.RoleId,
                ManagerId = dto.ManagerId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully",
                employeeCode = user.EmployeeCode
            });
        }

        // ===============================
        // DELETE USER
        // ===============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
    

// ===============================
// ASSIGN MANAGER
// ===============================
[HttpPut("assign-manager")]
        public async Task<IActionResult> AssignManager(int userId, int managerId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var manager = await _context.Users.FindAsync(managerId);

            if (manager == null)
                return NotFound("Manager not found");

            user.ManagerId = managerId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Manager assigned successfully" });
        }
    }
}
