namespace LeaveOTManagement.DTOs.Leave
{
    public class TeamCalendarDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}