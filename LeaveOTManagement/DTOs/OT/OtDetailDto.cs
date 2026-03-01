using Microsoft.AspNetCore.Mvc;

namespace LeaveOTManagement.DTOs.OT
{
    public class OtDetailDto
    {
        public DateOnly WorkDate { get; set; }
        public TimeOnly FromTime { get; set; }
        public TimeOnly ToTime { get; set; }
        public decimal Hours { get; set; }
    }
}
