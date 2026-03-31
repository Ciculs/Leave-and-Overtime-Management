namespace LeaveOTManagement.DTOs
{
    public class ForgotPasswordResetDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}