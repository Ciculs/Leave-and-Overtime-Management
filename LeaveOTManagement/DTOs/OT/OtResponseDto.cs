namespace LeaveOTManagement.DTOs.OT
{
    public class OtResponseDto
    {
        public long Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
        public string UserApprovalStatus { get; set; } = string.Empty;
        public int CurrentApprovalLevel { get; set; }
        public string RejectReason { get; set; } = string.Empty;
        public List<OtDetailDto> Details { get; set; } = new();
    }
}
