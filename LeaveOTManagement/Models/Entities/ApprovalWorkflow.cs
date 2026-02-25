using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class ApprovalWorkflow
{
    public int Id { get; set; }

    public string RequestType { get; set; } = null!;

    public int Level { get; set; }

    public int RoleId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Role Role { get; set; } = null!;
}
