using LeaveOTManagement.Data;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OTController : ControllerBase
{
    private readonly LeaveOTContext _context;

    public OTController(LeaveOTContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOT(Otrequest request)
    {
        var userId = int.Parse(User.FindFirst("id").Value);

        request.UserId = userId;
        request.Status = "PendingManager";

        _context.Otrequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(request);
    }

    [HttpGet("pending-manager")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> PendingManager()
    {
        return Ok(await _context.Otrequests
            .Where(x => x.Status == "PendingManager")
            .ToListAsync());
    }

    [HttpPost("manager-approve/{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ManagerApprove(int id)
    {
        var ot = await _context.Otrequests.FindAsync(id);
        ot.Status = "PendingHR";

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("hr-approve/{id}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> HRApprove(int id)
    {
        var ot = await _context.Otrequests.FindAsync(id);
        ot.Status = "Approved";

        await _context.SaveChangesAsync();
        return Ok();
    }
}