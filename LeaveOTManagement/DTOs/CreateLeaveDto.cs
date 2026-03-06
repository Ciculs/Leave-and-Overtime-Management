using System.ComponentModel.DataAnnotations;

namespace LeaveOTManagement.DTOs
{
    public class CreateLeaveDto
    {
        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateOnly FromDate { get; set; }

        [Required]
        public DateOnly ToDate { get; set; }

        [Required]
        public decimal TotalDays { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}