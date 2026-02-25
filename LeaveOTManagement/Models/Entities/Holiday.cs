using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class Holiday
{
    public int Id { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string? Name { get; set; }
}
