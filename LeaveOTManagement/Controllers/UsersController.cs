using LeaveOTManagement.Data;
using LeaveOTManagement.Models.DTOs;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "HR,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public UsersController(LeaveOTContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL USERS + SEARCH
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetUsers(string? search, string? role)
        {
            var query = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Include(u => u.Manager)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.EmployeeCode.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role.Name == role);
            }

            var users = await query
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
        // UPDATE USER
        // ===============================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return BadRequest("Department does not exist");

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return BadRequest("Role does not exist");

            user.FullName = dto.FullName;
            user.RoleId = dto.RoleId;
            user.DepartmentId = dto.DepartmentId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully" });
        }

        // ===============================
        // DEACTIVATE USER
        // ===============================
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == id);

            user.IsActive = false;

            if (account != null)
            {
                account.IsLocked = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully" });
        }

        // ===============================
        // ACTIVATE USER
        // ===============================
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == id);

            user.IsActive = true;

            if (account != null)
            {
                account.IsLocked = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User activated successfully" });
        }

        // ===============================
        // GET MANAGERS
        // ===============================
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Manager" && u.IsActive == true)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.EmployeeCode
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
        // CREATE USER + ACCOUNT
        // ===============================
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");

            var usernameExists = await _context.Accounts
                .AnyAsync(a => a.Username == dto.Username.Trim());

            if (usernameExists)
                return BadRequest("Username already exists");

            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return BadRequest("Department không tồn tại");

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return BadRequest("Role không tồn tại");

            if (dto.ManagerId.HasValue)
            {
                var managerExists = await _context.Users.AnyAsync(u => u.Id == dto.ManagerId.Value);
                if (!managerExists)
                    return BadRequest("Manager không tồn tại");
            }

            var lastUser = await _context.Users
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            string newCode = "EMP001";

            if (lastUser != null && !string.IsNullOrEmpty(lastUser.EmployeeCode) && lastUser.EmployeeCode.Length >= 6)
            {
                int number = int.Parse(lastUser.EmployeeCode.Substring(3));
                newCode = "EMP" + (number + 1).ToString("D3");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    EmployeeCode = newCode,
                    FullName = dto.FullName?.Trim(),
                    Email = dto.Email?.Trim(),
                    DepartmentId = dto.DepartmentId,
                    RoleId = dto.RoleId,
                    ManagerId = dto.ManagerId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var account = new Account
                {
                    Username = dto.Username.Trim(),
                    PasswordHash = dto.Password.Trim(),
                    UserId = user.Id,
                    IsLocked = false
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "User created successfully",
                    employeeCode = user.EmployeeCode
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Create user failed",
                    error = ex.Message
                });
            }
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

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

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

            var manager = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == managerId);

            if (manager == null)
                return NotFound("Manager not found");

            if (manager.Role?.Name != "Manager")
                return BadRequest("Selected user is not a manager");

            if (userId == managerId)
                return BadRequest("User cannot assign themselves as manager");

            user.ManagerId = managerId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Manager assigned successfully" });
        }
    }
}