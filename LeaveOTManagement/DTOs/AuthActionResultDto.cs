namespace LeaveOTManagement.DTOs
{
    public class AuthActionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ResetToken { get; set; }
    }
}