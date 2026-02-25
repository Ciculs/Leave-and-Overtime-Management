using Microsoft.AspNetCore.Mvc;

namespace LeaveOTManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetLeaves()
        {
            var fakeData = new[]
            {
                new { Id = 1, Reason = "Family trip", Status = "Pending" },
                new { Id = 2, Reason = "Sick leave", Status = "Approved" }
            };

            return Ok(fakeData);
        }
    }
}