using System.Threading.Tasks;

namespace LeaveOTManagement.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string subject, string otpCode, string purpose);
    }
}
