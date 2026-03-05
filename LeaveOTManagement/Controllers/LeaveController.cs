using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public LeaveController(LeaveOTContext context)
        {
            _context = context;
        }

        /* =====================================================
           GET LEAVE BALANCES
        ===================================================== */
        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = DateTime.Now.Year;

            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.UserId == userId && b.Year == currentYear)
                .Select(b => new
                {
                    LeaveTypeId = b.LeaveTypeId,
                    LeaveTypeName = b.LeaveType.Name,
                    TotalDays = b.TotalDays,
                    UsedDays = b.UsedDays,
                    RemainingDays = b.TotalDays - b.UsedDays
                }).ToListAsync();

            var unpaidLeave = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.MaxDaysPerYear == null);

            if (unpaidLeave != null)
            {
                balances.Add(new
                {
                    LeaveTypeId = unpaidLeave.Id,
                    LeaveTypeName = unpaidLeave.Name,
                    TotalDays = 0m,
                    UsedDays = (decimal?)0m,
                    RemainingDays = (decimal?)999m
                });
            }

            return Ok(balances);
        }

        /* =====================================================
           GET MY LEAVE REQUESTS
        ===================================================== */
        [HttpGet("my")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var leaves = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    Id = l.Id,
                    LeaveType = l.LeaveType.Name,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = l.Status,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(leaves);
        }

        /* =====================================================
           SUBMIT LEAVE REQUEST
        ===================================================== */
        [HttpPost]
        public async Task<IActionResult> SubmitLeave([FromBody] CreateLeaveDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            /* CHECK OVERLAPPING LEAVE */
            var isOverlapping = await _context.LeaveRequests
                .AnyAsync(r => r.UserId == userId
                            && r.Status != "Rejected"
                            && r.Status != "Cancelled"
                            && r.FromDate <= request.ToDate
                            && r.ToDate >= request.FromDate);

            if (isOverlapping)
            {
                return BadRequest(new
                {
                    message = "Lỗi: Bạn đã có đơn xin nghỉ khác trùng với khoảng thời gian này!"
                });
            }

            /* CHECK LEAVE BALANCE */
            var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);

            if (leaveType != null && leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId &&
                        b.LeaveTypeId == request.LeaveTypeId &&
                        b.Year == DateTime.Now.Year);

                if (balance == null || (balance.TotalDays - balance.UsedDays) < request.TotalDays)
                {
                    return BadRequest(new
                    {
                        message = "Lỗi: Số dư phép hiện tại của bạn không đủ!"
                    });
                }
            }

            /* CREATE LEAVE REQUEST */
            var newLeave = new LeaveRequest
            {
                UserId = userId,
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                Status = "Pending",
                CurrentApprovalLevel = 1,
                CreatedAt = DateTime.Now
            };

            _context.LeaveRequests.Add(newLeave);
            await _context.SaveChangesAsync();

            /* CREATE APPROVAL FOR MANAGER */
            var user = await _context.Users.FindAsync(userId);

            if (user?.ManagerId != null)
            {
                var approval = new Approval
                {
                    RequestId = newLeave.Id,
                    RequestType = "Leave",
                    ApprovalLevel = 1,
                    ApproverId = (int)user.ManagerId,
                    Status = "Pending"
                };

                _context.Approvals.Add(approval);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Gửi yêu cầu nghỉ phép thành công!"
            });
        }
    }
}