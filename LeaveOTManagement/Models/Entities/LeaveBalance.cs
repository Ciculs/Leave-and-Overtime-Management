using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class LeaveBalance
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LeaveTypeId { get; set; }

    public int Year { get; set; }

    public decimal TotalDays { get; set; }

    public decimal? UsedDays { get; set; }

    public virtual LeaveType LeaveType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
