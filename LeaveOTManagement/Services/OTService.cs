using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs.OT;
using LeaveOTManagement.Models.Entities;
using LeaveOTManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Services
{
    public class OTService : IOTService
    {
        private readonly LeaveOTContext _context;

        public OTService(LeaveOTContext context)
        {
            _context = context;
        }

        public async Task<long> CreateOtAsync(int userId, CreateOtRequestDto dto)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("OT must have at least one detail.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ot = new Otrequest
                {
                    UserId = userId,
                    Reason = dto.Reason,
                    Status = "Pending",
                    CurrentApprovalLevel = 1,
                    CreatedAt = DateTime.Now
                };

                _context.Otrequests.Add(ot);
                await _context.SaveChangesAsync();

                foreach (var d in dto.Details)
                {
                    if (d.ToTime <= d.FromTime)
                        throw new Exception("Invalid time range.");

                    var start = d.WorkDate.Date + d.FromTime;

                    if (start < DateTime.Now)
                        throw new Exception("Cannot create OT in the past.");

                    var hours = (decimal)(d.ToTime - d.FromTime).TotalHours;

                    _context.Otdetails.Add(new Otdetail
                    {
                        OtrequestId = ot.Id,
                        WorkDate = DateOnly.FromDateTime(d.WorkDate),
                        FromTime = TimeOnly.FromTimeSpan(d.FromTime),
                        ToTime = TimeOnly.FromTimeSpan(d.ToTime),
                        Hours = hours
                    });
                }

                await _context.SaveChangesAsync();

                await CreateApprovalWorkflow("OT", ot.Id);

                await transaction.CommitAsync();
                return ot.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOtAsync(long id, int userId, UpdateOtRequestDto dto)
        {
            var ot = await _context.Otrequests
                .Include(x => x.Otdetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (ot == null)
                throw new Exception("OT request not found.");

            if (ot.Status != "Pending")
                throw new Exception("Only pending request can be edited.");

            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("OT must have at least one detail.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                ot.Reason = dto.Reason;

                _context.Otdetails.RemoveRange(ot.Otdetails);

                foreach (var d in dto.Details)
                {
                    if (d.ToTime <= d.FromTime)
                        throw new Exception("Invalid time range.");

                    var start = d.WorkDate.Date + d.FromTime;

                    if (start < DateTime.Now)
                        throw new Exception("Cannot update OT in the past.");

                    var hours = (decimal)(d.ToTime - d.FromTime).TotalHours;

                    _context.Otdetails.Add(new Otdetail
                    {
                        OtrequestId = ot.Id,
                        WorkDate = DateOnly.FromDateTime(d.WorkDate),
                        FromTime = TimeOnly.FromTimeSpan(d.FromTime),
                        ToTime = TimeOnly.FromTimeSpan(d.ToTime),
                        Hours = hours
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OtResponseDto>> GetMyOtAsync(int userId, string? status)
        {
            var query = _context.Otrequests
                .Include(x => x.Otdetails)
                .Where(x => x.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x =>
                    x.Status != null &&
                    x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new OtResponseDto
                {
                    Id = x.Id,
                    Reason = x.Reason ?? "",
                    Status = x.Status ?? "",
                    CreatedAt = x.CreatedAt ?? DateTime.MinValue,
                    Details = x.Otdetails.Select(d => new OtDetailDto
                    {
                        WorkDate = d.WorkDate,
                        FromTime = d.FromTime,
                        ToTime = d.ToTime,
                        Hours = d.Hours
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<OtResponseDto?> GetOtByIdAsync(long id, int userId)
        {
            var ot = await _context.Otrequests
                .Include(x => x.Otdetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (ot == null)
                return null;

            return new OtResponseDto
            {
                Id = ot.Id,
                Reason = ot.Reason ?? "",
                Status = ot.Status ?? "",
                CreatedAt = ot.CreatedAt ?? DateTime.MinValue,
                Details = ot.Otdetails.Select(d => new OtDetailDto
                {
                    WorkDate = d.WorkDate,
                    FromTime = d.FromTime,
                    ToTime = d.ToTime,
                    Hours = d.Hours
                }).ToList()
            };
        }

        public async Task<List<OtResponseDto>> GetPendingApprovalsAsync(int approverId)
        {
            var pendingApprovals = await _context.Approvals
                .Where(a => a.ApproverId == approverId
                            && a.RequestType == "OT"
                            && a.Status == "Pending")
                .ToListAsync();

            if (!pendingApprovals.Any())
                return new List<OtResponseDto>();

            var requestIds = pendingApprovals
                .Select(a => a.RequestId)
                .Distinct()
                .ToList();

            var requests = await _context.Otrequests
                .Include(x => x.Otdetails)
                .Include(x => x.User)
                .Where(x => requestIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return requests
                .Where(x =>
                {
                    var myApproval = pendingApprovals.FirstOrDefault(a => a.RequestId == x.Id);
                    return myApproval != null && x.CurrentApprovalLevel == myApproval.ApprovalLevel;
                })
                .Select(x =>
                {
                    var myApproval = pendingApprovals.First(a => a.RequestId == x.Id);

                    return new OtResponseDto
                    {
                        Id = x.Id,
                        Reason = x.Reason ?? "",
                        Status = x.Status ?? "",
                        CreatedAt = x.CreatedAt ?? DateTime.MinValue,
                        EmployeeName = x.User?.FullName ?? "Unknown",
                        UserApprovalStatus = myApproval.Status ?? "Pending",
                        CurrentApprovalLevel = x.CurrentApprovalLevel ?? 1,
                        Details = x.Otdetails.Select(d => new OtDetailDto
                        {
                            WorkDate = d.WorkDate,
                            FromTime = d.FromTime,
                            ToTime = d.ToTime,
                            Hours = d.Hours
                        }).ToList()
                    };
                })
                .ToList();
        }

        public async Task ManagerApproveOtAsync(long requestId, int approverId)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                a.RequestId == requestId &&
                a.RequestType == "OT" &&
                a.ApproverId == approverId &&
                a.Status == "Pending" &&
                a.ApprovalLevel == 1);

            if (approval == null)
                throw new Exception("Manager approval not found.");

            var ot = await _context.Otrequests.FindAsync(requestId);

            if (ot == null)
                throw new Exception("OT request not found.");

            if (ot.CurrentApprovalLevel != 1 || ot.Status != "Pending")
                throw new Exception("Request is not waiting for manager approval.");

            approval.Status = "Approved";
            approval.ActionDate = DateTime.Now;

            ot.CurrentApprovalLevel = 2;
            ot.Status = "ManagerApproved";

            await _context.SaveChangesAsync();
        }

        public async Task HrApproveOtAsync(long requestId, int approverId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                    a.RequestId == requestId &&
                    a.RequestType == "OT" &&
                    a.ApproverId == approverId &&
                    a.Status == "Pending" &&
                    a.ApprovalLevel == 2);

                if (approval == null)
                    throw new Exception("HR/Admin approval not found.");

                var ot = await _context.Otrequests
                    .Include(x => x.Otdetails)
                    .FirstOrDefaultAsync(x => x.Id == requestId);

                if (ot == null)
                    throw new Exception("OT request not found.");

                if (ot.CurrentApprovalLevel != 2 || ot.Status != "ManagerApproved")
                    throw new Exception("Request is not waiting for HR/Admin approval.");

                approval.Status = "Approved";
                approval.ActionDate = DateTime.Now;

                ot.Status = "Approved";
                ot.CurrentApprovalLevel = 3;

                AddToPayroll(ot);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void AddToPayroll(Otrequest ot)
        {
            if (ot.Otdetails == null || !ot.Otdetails.Any())
                return;

            foreach (var d in ot.Otdetails)
            {
                var payroll = new PayrollLog
                {
                    UserId = ot.UserId,
                    OTRequestId = ot.Id,
                    WorkDate = d.WorkDate.ToDateTime(TimeOnly.MinValue),
                    Hours = d.Hours,
                    RateMultiplier = 1.5m,
                    CreatedAt = DateTime.Now
                };

                _context.PayrollLogs.Add(payroll);
            }
        }

        public async Task RejectOtAsync(long requestId, int approverId, string reason)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                a.RequestId == requestId &&
                a.RequestType == "OT" &&
                a.ApproverId == approverId &&
                a.Status == "Pending");

            if (approval == null)
                throw new Exception("Approval not found.");

            var ot = await _context.Otrequests.FindAsync(requestId);

            if (ot == null)
                throw new Exception("OT request not found.");

            approval.Status = "Rejected";
            approval.Comment = reason;
            approval.ActionDate = DateTime.Now;

            ot.Status = "Rejected";
            ot.CurrentApprovalLevel = -1;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TeamOtCalendarDto>> GetTeamOtCalendarAsync(int managerUserId, int year, int month)
        {
            if (month < 1 || month > 12)
                throw new Exception("Invalid month.");

            var teamMemberIds = await _context.Users
                .Where(u => u.ManagerId == managerUserId)
                .Select(u => u.Id)
                .ToListAsync();

            if (!teamMemberIds.Any())
                return new List<TeamOtCalendarDto>();

            var result = await _context.Otrequests
                .Include(o => o.User)
                .Include(o => o.Otdetails)
                .Where(o => teamMemberIds.Contains(o.UserId))
                .Where(o => o.Otdetails.Any(d => d.WorkDate.Year == year && d.WorkDate.Month == month))
                .SelectMany(o => o.Otdetails
                    .Where(d => d.WorkDate.Year == year && d.WorkDate.Month == month)
                    .Select(d => new TeamOtCalendarDto
                    {
                        Id = o.Id,
                        EmployeeName = o.User.FullName,
                        WorkDate = d.WorkDate,
                        FromTime = d.FromTime.ToString(),
                        ToTime = d.ToTime.ToString(),
                        Reason = o.Reason,
                        Status = o.Status
                    }))
                .ToListAsync();

            return result;
        }

        private async Task CreateApprovalWorkflow(string requestType, long requestId)
        {
            var workflows = await _context.ApprovalWorkflows
                .Where(x => x.RequestType == requestType)
                .OrderBy(x => x.Level)
                .ToListAsync();

            foreach (var wf in workflows)
            {
                var approvers = await _context.Users
                    .Where(u => u.RoleId == wf.RoleId)
                    .ToListAsync();

                foreach (var approver in approvers)
                {
                    _context.Approvals.Add(new Approval
                    {
                        RequestId = requestId,
                        RequestType = requestType,
                        ApproverId = approver.Id,
                        ApprovalLevel = wf.Level,
                        Status = "Pending"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}