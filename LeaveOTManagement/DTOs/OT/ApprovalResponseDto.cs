namespace LeaveOTManagement.DTOs.OT
{
    public class ApprovalResponseDto
    {
        public long Id { get; set; }
        public int ApprovalLevel { get; set; }
        public string ApproverName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ActionDate { get; set; }
        public string? Comment { get; set; }
    }
}
