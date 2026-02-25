using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<ApprovalWorkflow> ApprovalWorkflows { get; set; } = new List<ApprovalWorkflow>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
