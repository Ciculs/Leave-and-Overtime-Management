using LeaveOTManagement.Data;
using LeaveOTManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "HR,Admin")]
public class ReportController : ControllerBase
{
    private readonly LeaveOTContext _context;
    private readonly IOTService _otService;

    public ReportController(LeaveOTContext context, IOTService otService)
    {
        _context = context;
        _otService = otService;
    }

    [HttpGet("top-ot")]
    public IActionResult TopOT()
    {
        var data = _context.Otdetails
            .Where(d => d.Otrequest.Status == "Approved")
            .GroupBy(d => new
            {
                d.Otrequest.UserId,
                d.Otrequest.User.FullName
            })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                FullName = g.Key.FullName,
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
            .Where(d => d.WorkDate.Month == month && d.WorkDate.Year == year)
            .Select(d => new
            {
                RequestId = d.Otrequest.Id,
                UserId = d.Otrequest.UserId,
                FullName = d.Otrequest.User.FullName,
                Date = d.WorkDate,
                Hours = d.Hours,
                Status = d.Otrequest.Status
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        return Ok(data);
    }

    [HttpGet("download")]
    public IActionResult DownloadReport(int month, int year)
    {
        var data = _context.Otdetails
            .Where(d => d.WorkDate.Month == month && d.WorkDate.Year == year)
            .Select(d => new
            {
                UserId = d.Otrequest.UserId,
                FullName = d.Otrequest.User.FullName,
                Date = d.WorkDate,
                Hours = d.Hours,
                Status = d.Otrequest.Status
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        var csv = new StringBuilder();
        csv.AppendLine("UserId,FullName,Date,Hours,Status");

        foreach (var r in data)
        {
            var safeName = (r.FullName ?? "").Replace(",", " ");
            csv.AppendLine($"{r.UserId},{safeName},{r.Date:yyyy-MM-dd},{r.Hours},{r.Status}");
        }

        return File(
            Encoding.UTF8.GetBytes(csv.ToString()),
            "text/csv",
            $"OT_Report_{month}_{year}.csv"
        );
    }
}