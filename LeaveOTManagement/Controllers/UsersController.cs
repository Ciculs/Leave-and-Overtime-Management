using Microsoft.AspNetCore.Mvc;
using LeaveOTManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UsersController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public UsersController(LeaveOTContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }
    }
}