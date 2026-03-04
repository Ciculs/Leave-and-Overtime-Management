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
        [MaxLength(1000, ErrorMessage = "Lý do không được quá 1000 ký tự")]
        public string Reason { get; set; }
    }
}