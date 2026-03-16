using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs;
using LeaveOTManagement.Models.Entities;
using LeaveOTManagement.Services;
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
        private readonly LeaveService _leaveService;

        public LeaveController(LeaveOTContext context, LeaveService leaveService)
        {
            _context = context;
            _leaveService = leaveService;
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
                })
                .ToListAsync();

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
           MANAGER LEAVE CONTROLLER STATS
        ===================================================== */
        [HttpGet("manager-stats")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerStats()
        {
            var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var pendingLeave = await _context.LeaveRequests
                .Include(l => l.User)
                .CountAsync(l => l.Status == "Pending" && l.CurrentApprovalLevel == 1 && l.User.ManagerId == managerId);

            var pendingOt = await _context.Otrequests
                .Include(o => o.User)
                .CountAsync(o => o.Status == "Pending" && o.CurrentApprovalLevel == 1 && o.User.ManagerId == managerId);

            var teamMembers = await _context.Users
                .CountAsync(u => u.ManagerId == managerId && u.IsActive == true);

            return Ok(new
            {
                pendingLeave = pendingLeave,
                pendingOt = pendingOt,
                teamMembers = teamMembers
            });
        }
        
        /* =====================================================
           MANAGER LEAVE CONTROLLER
        ===================================================== */
        [HttpPut("manager-approve/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagerApprove(long id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            leave.Status = "Pending";
            leave.CurrentApprovalLevel = 2;

            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.RequestId == id && a.ApprovalLevel == 1 && a.RequestType == "Leave");

            if (approval != null)
            {
                approval.Status = "Approved";
            }

            var hrUsers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "HR")
                .ToListAsync();

            foreach (var hr in hrUsers)
            {
                _context.Approvals.Add(new Approval
                {
                    RequestId = leave.Id,
                    RequestType = "Leave",
                    ApprovalLevel = 2,
                    ApproverId = hr.Id,
                    Status = "Pending"
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("manager-reject/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagerReject(long id, [FromBody] RejectDto dto)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            if (leave.Status == "Rejected") return BadRequest(new { message = "Đơn này đã bị từ chối rồi!" });

            leave.Status = "Rejected";
            leave.Reason = leave.Reason + " | Rejected by Manager: " + dto.Reason;

            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.RequestId == id && a.ApprovalLevel == 1 && a.RequestType == "Leave");

            if (approval != null) approval.Status = "Rejected";

            // ✅ HOÀN TRẢ LẠI NGÀY PHÉP (REFUND)
            var leaveType = await _context.LeaveTypes.FindAsync(leave.LeaveTypeId);
            if (leaveType != null && leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserId == leave.UserId && b.LeaveTypeId == leave.LeaveTypeId && b.Year == leave.FromDate.Year);

                if (balance != null)
                {
                    decimal daysToRefund = (decimal)leave.TotalDays;
                    if (daysToRefund <= 0) daysToRefund = (leave.ToDate.DayNumber - leave.FromDate.DayNumber) + 1m;
                    
                    balance.UsedDays -= daysToRefund;
                    if (balance.UsedDays < 0) balance.UsedDays = 0; // Đảm bảo không bị âm
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        /* =====================================================
           MANAGER GET PENDING
        ===================================================== */
        [HttpGet("pending-manager")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingForManager()
        {
            var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == "Pending" && l.CurrentApprovalLevel == 1 && l.User.ManagerId == managerId)
                .Select(l => new
                {
                    Id = l.Id,
                    EmployeeName = l.User.FullName,
                    LeaveType = l.LeaveType.Name,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = l.Status
                })
                .ToListAsync();

            return Ok(leaves);
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
                    Status = l.Status == "Pending" && l.CurrentApprovalLevel == 2 ? "Pending HR" : l.Status,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(leaves);
        }

        /* =====================================================
           SUBMIT LEAVE REQUEST
        ===================================================== */

        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> SubmitLeave([FromBody] CreateLeaveDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            decimal safeTotalDays = request.TotalDays; 
            if (safeTotalDays <= 0)
            {
                safeTotalDays = (request.ToDate.DayNumber - request.FromDate.DayNumber) + 1m;
            }

            var isOverlapping = await _context.LeaveRequests
                .AnyAsync(r => r.UserId == userId
                            && r.Status != "Rejected"
                            && r.Status != "Cancelled"
                            && r.FromDate <= request.ToDate
                            && r.ToDate >= request.FromDate);

            if (isOverlapping)
            {
                return BadRequest(new { message = "Lỗi: Bạn đã có đơn xin nghỉ khác trùng với khoảng thời gian này!" });
            }

            var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);
            if (leaveType != null && leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserId == userId 
                                           && b.LeaveTypeId == request.LeaveTypeId 
                                           && b.Year == DateTime.Now.Year);

                if (balance == null) return BadRequest(new { message = "Lỗi: Không tìm thấy dữ liệu ngày phép của bạn năm nay!" });

                var availableDays = balance.TotalDays - balance.UsedDays;

                if (availableDays < safeTotalDays)
                {
                    return BadRequest(new { message = $"Lỗi: Bạn chỉ còn {availableDays} ngày phép. Không thể xin nghỉ {safeTotalDays} ngày!" });
                }

                // ✅ TRỪ PHÉP NGAY LẬP TỨC
                balance.UsedDays += safeTotalDays;
            }

            var newLeave = new LeaveRequest
            {
                UserId = userId,
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = safeTotalDays, 
                Reason = request.Reason,
                Status = "Pending",
                CurrentApprovalLevel = 1,
                CreatedAt = DateTime.Now
            };

            _context.LeaveRequests.Add(newLeave);
            await _context.SaveChangesAsync();

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

        /* =====================================================
           TEAM CALENDAR (MANAGER)
        ===================================================== */

        [HttpGet("team-calendar")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetTeamCalendar(int year, int month)
        {
            var managerId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"
            );

            var data = await _leaveService.GetTeamCalendar(managerId, year, month);

            return Ok(data);
        }

        /* =====================================================
           HR APPROVAL ENDPOINTS 
        ===================================================== */
        
        [HttpGet("pending-hr")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GetPendingForHR()
        {
            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == "Pending" && l.CurrentApprovalLevel == 2)
                .Select(l => new
                {
                    Id = l.Id,
                    EmployeeName = l.User.FullName,
                    LeaveType = l.LeaveType.Name,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = "Pending HR"
                })
                .ToListAsync();

            return Ok(leaves);
        }

        [HttpPut("hr-approve/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> HRApprove(long id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            if (leave.Status == "Approved") 
            {
                return BadRequest(new { message = "Đơn này đã được duyệt rồi!" });
            }

            // ✅ KHÔNG CẦN TRỪ PHÉP Ở ĐÂY NỮA VÌ ĐÃ TRỪ TỪ LÚC TẠO ĐƠN
            leave.Status = "Approved";

            var hrId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.RequestId == id 
                                       && a.ApprovalLevel == 2 
                                       && a.RequestType == "Leave" 
                                       && a.ApproverId == hrId);

            if (approval != null)
            {
                approval.Status = "Approved";
                approval.ActionDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("hr-reject/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> HRReject(long id, [FromBody] RejectDto dto)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            if (leave.Status == "Rejected") return BadRequest(new { message = "Đơn này đã bị từ chối rồi!" });

            leave.Status = "Rejected";
            leave.Reason = leave.Reason + " | Rejected by HR: " + dto.Reason;

            var hrId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.RequestId == id && a.ApprovalLevel == 2 && a.RequestType == "Leave" && a.ApproverId == hrId);

            if (approval != null)
            {
                approval.Status = "Rejected";
                approval.ActionDate = DateTime.Now;
            }

            // ✅ HOÀN TRẢ LẠI NGÀY PHÉP (REFUND)
            var leaveType = await _context.LeaveTypes.FindAsync(leave.LeaveTypeId);
            if (leaveType != null && leaveType.MaxDaysPerYear != null)
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserId == leave.UserId && b.LeaveTypeId == leave.LeaveTypeId && b.Year == leave.FromDate.Year);

                if (balance != null)
                {
                    decimal daysToRefund = (decimal)leave.TotalDays;
                    if (daysToRefund <= 0) daysToRefund = (leave.ToDate.DayNumber - leave.FromDate.DayNumber) + 1m;
                    
                    balance.UsedDays -= daysToRefund;
                    if (balance.UsedDays < 0) balance.UsedDays = 0; // Đảm bảo không bị âm
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}