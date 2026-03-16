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

        // 🔥 DÁN CreateOtAsync vào đây
        public async Task<long> CreateOtAsync(int userId, CreateOtRequestDto dto)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("OT must have at least one detail");

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
                        throw new Exception("Invalid time range");

                    var start = d.WorkDate.Date + d.FromTime;

                    if (start < DateTime.Now)
                        throw new Exception("Cannot create OT in the past");

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

        // 🔥 DÁN UpdateOtAsync vào đây
        public async Task UpdateOtAsync(long id, int userId, UpdateOtRequestDto dto)
        {
            var ot = await _context.Otrequests
                .Include(x => x.Otdetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (ot == null)
                throw new Exception("OT request not found");

            if (ot.Status != "Pending")
                throw new Exception("Only pending request can be edited");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                ot.Reason = dto.Reason;

                // Xóa detail cũ
                _context.Otdetails.RemoveRange(ot.Otdetails);

                foreach (var d in dto.Details)
                {
                    if (d.ToTime <= d.FromTime)
                        throw new Exception("Invalid time range");

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

        // 🔥 DÁN GetMyOtAsync vào đây
        public async Task<List<OtResponseDto>> GetMyOtAsync(int userId, string? status)
        {
            var query = _context.Otrequests
                .Include(x => x.Otdetails)
                .Where(x => x.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
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
                    Reason = x.Reason,
                    Status = x.Status,
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

        // 🔥 DÁN CreateApprovalWorkflow vào đây (private method)
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
            var approvals = await _context.Approvals
                .Where(a =>
                    a.ApproverId == approverId &&
                    a.RequestType == "OT")
                .Select(a => new
                {
                    a.RequestId,
                    a.Status,
                    a.ApprovalLevel
                })
                .ToListAsync();

            if (!approvals.Any())
                return new List<OtResponseDto>();

            var requestIds = approvals
                .Select(a => a.RequestId)
                .Distinct()
                .ToList();

            var requests = await _context.Otrequests
                .Include(x => x.Otdetails)
                .Where(x => requestIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = requests
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
                .ToList();

            return result;
        }

        public async Task ManagerApproveOtAsync(long requestId, int approverId)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                a.RequestId == requestId &&
                a.RequestType == "OT" &&
                a.ApproverId == approverId &&
                a.Status == "Pending");

            if (approval == null)
                throw new Exception("Approval not found");

            var ot = await _context.Otrequests.FindAsync(requestId);

            if (ot == null)
                throw new Exception("OT request not found");

            if (ot.CurrentApprovalLevel != 1)
                throw new Exception("Manager approval already processed");

            // Manager approve
            approval.Status = "Approved";
            approval.ActionDate = DateTime.Now;

            // Move workflow to HR
            ot.CurrentApprovalLevel = 2;
            ot.Status = "ManagerApproved";

            // 🔥 CREATE HR APPROVAL RECORD
            var hrUsers = await _context.Users
                .Where(u => u.RoleId == 3)
                 .ToListAsync();

            foreach (var hr in hrUsers)
            {
                _context.Approvals.Add(new Approval
                {
                    RequestId = requestId,
                    RequestType = "OT",
                    ApproverId = hr.Id,
                    ApprovalLevel = 2,
                    Status = "Pending"
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task HrApproveOtAsync(long requestId, int approverId)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                a.RequestId == requestId &&
                a.RequestType == "OT" &&
                a.ApproverId == approverId &&
                a.Status == "Pending");

            if (approval == null)
                throw new Exception("Approval not found");

            var ot = await _context.Otrequests
                .Include(x => x.Otdetails)
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (ot == null)
                throw new Exception("OT request not found");

            if (ot.CurrentApprovalLevel != 2)
                throw new Exception("Manager must approve first");

            approval.Status = "Approved";
            approval.ActionDate = DateTime.Now;

            // FINAL APPROVAL
            ot.CurrentApprovalLevel = 3;
            ot.Status = "Approved";

            await _context.SaveChangesAsync();

            await AddToPayroll(ot);
        }

        public async Task RejectOtAsync(long requestId, int approverId, string reason)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a =>
                a.RequestId == requestId &&
                a.RequestType == "OT" &&
                a.ApproverId == approverId &&
                a.Status == "Pending");

            if (approval == null)
                throw new Exception("Approval not found");

            var ot = await _context.Otrequests.FindAsync(requestId);

            if (ot == null)
                throw new Exception("OT request not found");

            // Reject record
            approval.Status = "Rejected";
            approval.Comment = reason;
            approval.ActionDate = DateTime.Now;

            // Stop workflow
            ot.Status = "Rejected";         
            ot.CurrentApprovalLevel = -1;

            await _context.SaveChangesAsync();
        }

        private async Task AddToPayroll(Otrequest ot)
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

            await _context.SaveChangesAsync();
        }
    }
}