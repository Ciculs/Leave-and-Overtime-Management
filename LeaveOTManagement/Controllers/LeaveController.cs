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
           GET BALANCES
        ===================================================== */
        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId == 0)
                return Unauthorized(new { message = "Invalid user token." });

            var currentYear = DateTime.UtcNow.Year;

            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.UserId == userId && b.Year == currentYear)
                .Select(b => new
                {
                    leaveTypeId = b.LeaveTypeId,
                    leaveTypeName = b.LeaveType.Name,
                    totalDays = b.TotalDays,
                    usedDays = b.UsedDays,
                    remainingDays = b.TotalDays - b.UsedDays
                })
                .ToListAsync();

            // Thêm Unpaid Leave nếu có
            var unpaidLeave = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.MaxDaysPerYear == null);

            if (unpaidLeave != null)
            {
                balances.Add(new
                {
                    leaveTypeId = unpaidLeave.Id,
                    leaveTypeName = unpaidLeave.Name,
                    totalDays = 0m,
                    usedDays = (decimal?)0m,
                    remainingDays = (decimal?)999m
                });
            }

            return Ok(balances);
        }
        /* =====================================================
        GET MY LEAVE REQUESTS
        ===================================================== */
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || userId == 0)
                return Unauthorized(new { message = "Invalid user token." });

            var leaves = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    id = l.Id,
                    leaveType = l.LeaveType.Name,
                    fromDate = l.FromDate,
                    toDate = l.ToDate,
                    totalDays = l.TotalDays,
                    reason = l.Reason,
                    status = l.Status
                })
                .ToListAsync();

            return Ok(leaves);
        }

        /* =====================================================
           SUBMIT LEAVE
        ===================================================== */
        [HttpPost]
        public async Task<IActionResult> SubmitLeave([FromBody] CreateLeaveDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId == 0)
                return Unauthorized(new { message = "Invalid user token." });

            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            // 1️⃣ Validate ngày trước
            if (request.FromDate > request.ToDate)
                return BadRequest(new { message = "Start date cannot be after end date." });

            var calculatedDays = (request.ToDate.DayNumber - request.FromDate.DayNumber) + 1;

            if (calculatedDays <= 0)
                return BadRequest(new { message = "Invalid leave duration." });

            // 2️⃣ Check LeaveType tồn tại
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Id == request.LeaveTypeId);

            if (leaveType == null)
                return BadRequest(new { message = "Leave type does not exist." });

            // 3️⃣ Check trùng lịch
            var isOverlapping = await _context.LeaveRequests
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.Status != "Rejected" &&
                    r.Status != "Cancelled" &&
                    r.FromDate <= request.ToDate &&
                    r.ToDate >= request.FromDate);

            if (isOverlapping)
                return BadRequest(new { message = "You already have a leave request overlapping this period." });

            // 4️⃣ Check số dư (nếu không phải Unpaid)
            if (leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId &&
                        b.LeaveTypeId == request.LeaveTypeId &&
                        b.Year == DateTime.UtcNow.Year);

                if (balance == null)
                    return BadRequest(new { message = "Leave balance not found." });

                var remaining = balance.TotalDays - balance.UsedDays;

                if (remaining < calculatedDays)
                {
                    return BadRequest(new
                    {
                        message = $"Insufficient balance. You need {calculatedDays} days but only have {remaining} days left."
                    });
                }

                // Trừ UsedDays luôn nếu muốn trừ ngay khi submit
                balance.UsedDays += calculatedDays;
            }

            // 5️⃣ Transaction đảm bảo atomic
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newLeave = new LeaveRequest
                {
                    UserId = userId,
                    LeaveTypeId = request.LeaveTypeId,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    TotalDays = calculatedDays,
                    Reason = request.Reason,
                    Status = "Pending",
                    CurrentApprovalLevel = 1,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeaveRequests.Add(newLeave);
                await _context.SaveChangesAsync();

                // Tạo approval nếu có manager
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

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }

            return Ok(new { message = "Leave request submitted successfully!" });
        }
    }
}