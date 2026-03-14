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

    [HttpGet("leave-trends")]
    public IActionResult LeaveTrends()
    {
        var data = _context.LeaveRequests
            .Where(x => x.Status == "Approved")
            .GroupBy(x => x.FromDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                TotalLeaves = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToList();

        return Ok(data);
    }

    [HttpGet("filter")]
    public IActionResult FilterReport(int month, int year)
    {
        var data = _context.Otdetails
            .Where(d =>
                d.WorkDate.Month == month &&
                d.WorkDate.Year == year)
            .Select(d => new
            {
                RequestId = d.Otrequest.Id,
                UserId = d.Otrequest.UserId,
                Date = d.WorkDate,
                Hours = d.Hours,
                Status = d.Otrequest.Status
            })
            .ToList();

        return Ok(data);
    }

    [HttpGet("download")]
    public IActionResult DownloadReport(int month, int year)
    {
        var data = _context.Otdetails
            .Where(d =>
                d.WorkDate.Month == month &&
                d.WorkDate.Year == year)
            .Select(d => new
            {
                UserId = d.Otrequest.UserId,
                Date = d.WorkDate,
                Hours = d.Hours,
                Status = d.Otrequest.Status
            })
            .ToList();

        var csv = new System.Text.StringBuilder();

        csv.AppendLine("UserId,Date,Hours,Status");

        foreach (var r in data)
        {
            csv.AppendLine($"{r.UserId},{r.Date},{r.Hours},{r.Status}");
        }

        return File(
            System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
            "text/csv",
            "OT_Report.csv"
        );
    }
}