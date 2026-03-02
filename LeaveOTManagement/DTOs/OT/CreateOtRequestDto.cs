namespace LeaveOTManagement.DTOs.OT
{
    public class CreateOtRequestDto
    {
        public string Reason { get; set; } = string.Empty;
        public List<CreateOtDetailDto> Details { get; set; } = new();
    }

    public class CreateOtDetailDto
    {
        public DateTime WorkDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
    }
}