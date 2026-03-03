using LeaveOTManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "HR")]
public class ReportController : ControllerBase
{
    private readonly LeaveOTContext _context;

    public ReportController(LeaveOTContext context)
    {
        _context = context;
    }

    [HttpGet("top-ot")]
    public IActionResult TopOT()
    {
        var data = _context.Otdetails
            .Where(d => d.Otrequest.Status == "Approved")
            .GroupBy(d => d.Otrequest.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalHours = g.Sum(x => x.Hours)
            })
            .OrderByDescending(x => x.TotalHours)
            .Take(5)
            .ToList();

        return Ok(data);
    }
}