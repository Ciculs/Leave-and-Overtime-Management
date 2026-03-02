namespace LeaveOTManagement.DTOs.OT
{
    public class UpdateOtRequestDto
    {
        public string Reason { get; set; } = string.Empty;
        public List<CreateOtDetailDto> Details { get; set; } = new();
    }
}
