using System;

namespace LeaveOTManagement.Models.Entities;

public partial class Holiday
{
    public int Id { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string? Name { get; set; }

    // true = ngày nghỉ chính thức
    public bool IsDayOff { get; set; } = true;

    // true = không cho xin nghỉ vào ngày này
    public bool IsLeaveBlocked { get; set; } = false;

    // API / Manual / Import
    public string Source { get; set; } = "API";

    public string? Note { get; set; }
}