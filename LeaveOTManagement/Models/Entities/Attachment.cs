using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Attachment
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public string EntityType { get; set; } = null!;

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? UploadedAt { get; set; }
}
