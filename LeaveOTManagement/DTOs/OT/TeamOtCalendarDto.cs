namespace LeaveOTManagement.DTOs.OT
{
    public class TeamOtCalendarDto
    {
        public long Id { get; set; }
        public string? EmployeeName { get; set; }
        public DateOnly WorkDate { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }
}