namespace LeaveOTManagement.DTOs
{
    public class ConfirmChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}