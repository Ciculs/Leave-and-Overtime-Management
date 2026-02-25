using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Otdetail
{
    public long Id { get; set; }

    public long OtrequestId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly FromTime { get; set; }

    public TimeOnly ToTime { get; set; }

    public decimal Hours { get; set; }

    public virtual Otrequest Otrequest { get; set; } = null!;
}
