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
                        WorkDate = d.WorkDate.ToString("yyyy-MM-dd"),
                        FromTime = d.FromTime.ToString("HH:mm"),
                        ToTime = d.ToTime.ToString("HH:mm"),
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
                    WorkDate = d.WorkDate.ToString("yyyy-MM-dd"),
                    FromTime = d.FromTime.ToString("HH:mm"),
                    ToTime = d.ToTime.ToString("HH:mm"),
                    Hours = d.Hours
                }).ToList()
            };
        }
    }
}