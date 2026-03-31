namespace LeaveOTManagement.Models.Entities
{
    public class EmailOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string OtpCode { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty; // ChangePassword

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}