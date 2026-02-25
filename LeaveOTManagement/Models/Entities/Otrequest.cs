using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Otrequest
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? CurrentApprovalLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Otdetail> Otdetails { get; set; } = new List<Otdetail>();

    public virtual User User { get; set; } = null!;
}
