using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveOTManagement.Data;

namespace LeaveOTManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public PayrollController(LeaveOTContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayroll()
        {
            var data = await _context.PayrollLogs
                .Include(p => p.User)
                .Select(p => new
                {
                    p.Id,
                    Employee = p.User.FullName,
                    p.WorkDate,
                    p.Hours,
                    p.RateMultiplier
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}