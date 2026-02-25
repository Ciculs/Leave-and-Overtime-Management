using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class LeaveType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsPaid { get; set; }

    public int? MaxDaysPerYear { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
