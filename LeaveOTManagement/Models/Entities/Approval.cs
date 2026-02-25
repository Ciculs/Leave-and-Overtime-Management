using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Approval
{
    public long Id { get; set; }

    public long RequestId { get; set; }

    public string RequestType { get; set; } = null!;

    public int ApprovalLevel { get; set; }

    public int ApproverId { get; set; }

    public string? Status { get; set; }

    public string? Comment { get; set; }

    public DateTime? ActionDate { get; set; }

    public virtual User Approver { get; set; } = null!;
}
