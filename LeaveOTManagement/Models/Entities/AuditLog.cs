using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class AuditLog
{
    public long Id { get; set; }

    public long? EntityId { get; set; }

    public string? EntityType { get; set; }

    public string? Action { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ActionBy { get; set; }

    public DateTime? ActionDate { get; set; }

    public virtual User? ActionByNavigation { get; set; }
}
