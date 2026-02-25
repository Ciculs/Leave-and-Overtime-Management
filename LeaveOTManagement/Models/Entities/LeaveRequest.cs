using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class LeaveRequest
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public decimal? TotalDays { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? CurrentApprovalLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LeaveType LeaveType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
