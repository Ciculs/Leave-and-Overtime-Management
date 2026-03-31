namespace LeaveOTManagement.DTOs
{
    public class ForgotPasswordVerifyOtpDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}