<<<<<<< HEAD
﻿using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveOTManagement.Data.LeaveOTContext _context;

        public LeaveController(LeaveOTManagement.Data.LeaveOTContext context)
        {
            _context = context;
        }

        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetLeaveBalance(int userId)
        {
            int currentYear = DateTime.Now.Year;
            var balanceRecord = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == currentYear && b.LeaveTypeId == 1);

            if (balanceRecord == null) return NotFound(new { message = "Leave data not found." });

            return Ok(new
            {
                totalDays = balanceRecord.TotalDays,
                usedDays = balanceRecord.UsedDays,
                balance = balanceRecord.TotalDays - balanceRecord.UsedDays
            });
        }

        [HttpGet("list/{userId}")]
        public async Task<IActionResult> GetLeaveList(int userId)
        {
            var leaves = await _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new {
                    id = l.Id,
                    fromDate = l.FromDate,
                    toDate = l.ToDate,
                    totalDays = l.TotalDays,
                    reason = l.Reason,
                    status = l.Status
                })
                .ToListAsync();
// Test commit change
            return Ok(leaves);
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestDto request)
        {
            decimal requestedDays = (decimal)(request.ToDate.Date - request.FromDate.Date).TotalDays + 1;

            if (requestedDays <= 0)
                return BadRequest(new { message = "End date must be greater than or equal to start date." });

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { message = "User not found." });
            if (user.ManagerId == null) return BadRequest(new { message = "Error: This employee does not have a Manager to approve the request." });

            int currentYear = DateTime.Now.Year;
            var balanceRecord = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Year == currentYear && b.LeaveTypeId == 1);

            if (balanceRecord == null) return BadRequest(new { message = "Leave balance not found." });

            decimal availableBalance = balanceRecord.TotalDays - (balanceRecord.UsedDays ?? 0);
            if (requestedDays > availableBalance)
            {
                return BadRequest(new { message = $"Insufficient balance! You only have {availableBalance} days left." });
            }

            var fromDateOnly = DateOnly.FromDateTime(request.FromDate);
            var toDateOnly = DateOnly.FromDateTime(request.ToDate);

            bool isOverlapping = await _context.LeaveRequests
                .AnyAsync(r => r.UserId == request.UserId
                            && (r.Status == "Pending" || r.Status == "Approved")
                            && r.FromDate <= toDateOnly
                            && r.ToDate >= fromDateOnly);

            if (isOverlapping)
            {
                return BadRequest(new { message = "This period overlaps with another pending or approved request!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newLeave = new LeaveRequest
                {
                    UserId = request.UserId,
                    LeaveTypeId = 1,
                    FromDate = fromDateOnly,
                    ToDate = toDateOnly,
                    TotalDays = requestedDays,
                    Reason = request.Reason,
                    Status = "Pending",
                    CurrentApprovalLevel = 1,
                    CreatedAt = DateTime.Now
                };

                _context.LeaveRequests.Add(newLeave);
                await _context.SaveChangesAsync();

                var newApproval = new Approval
                {
                    RequestId = newLeave.Id,
                    RequestType = "Leave",
                    ApprovalLevel = 1,
                    ApproverId = user.ManagerId.Value,
                    Status = "Pending"
                };

                _context.Approvals.Add(newApproval);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = "Leave request created successfully!" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "System error while saving the request." });
            }
        }
    }

    public class LeaveRequestDto
    {
        public int UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; }
    }
=======
﻿using LeaveOTManagement.Data;
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

            var unpaidLeave = await _context.LeaveTypes.FirstOrDefaultAsync(l => l.MaxDaysPerYear == null);
            if (unpaidLeave != null)
            {
                balances.Add(new
                {
                    LeaveTypeId = unpaidLeave.Id,
                    LeaveTypeName = unpaidLeave.Name,
                    TotalDays = 0m,
                    UsedDays = (decimal?)0m,         // Đã ép kiểu null để khớp với List
                    RemainingDays = (decimal?)999m   // Đã ép kiểu null để khớp với List
                });
            }

            return Ok(balances);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitLeave([FromBody] CreateLeaveDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Validate trùng ngày 
            var isOverlapping = await _context.LeaveRequests
                .AnyAsync(r => r.UserId == userId
                            && r.Status != "Rejected" && r.Status != "Cancelled"
                            && r.FromDate <= request.ToDate
                            && r.ToDate >= request.FromDate);

            if (isOverlapping)
                return BadRequest(new { message = "Lỗi: Bạn đã có đơn xin nghỉ khác trùng với khoảng thời gian này!" });

            // Validate số dư
            var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);
            if (leaveType != null && leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.LeaveTypeId == request.LeaveTypeId && b.Year == DateTime.Now.Year);

                if (balance == null || (balance.TotalDays - balance.UsedDays) < request.TotalDays)
                {
                    return BadRequest(new { message = "Lỗi: Số dư phép hiện tại của bạn không đủ!" });
                }
            }

            
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

            // Sinh Approval cho Manager
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

            return Ok(new { message = "Gửi yêu cầu nghỉ phép thành công!" });
        }
    }
>>>>>>> main
}