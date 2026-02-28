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
        var data = _context.Otrequests
            .Where(r => r.Status == "Approved")
            .Select(r => new
            {
                r.UserId,
                TotalHours = r.Otdetails.Sum(d => d.Hours)
            })
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalHours = g.Sum(x => x.TotalHours)
            })
            .OrderByDescending(x => x.TotalHours)
            .Take(5)
            .ToList();

        return Ok(data);
    }
}