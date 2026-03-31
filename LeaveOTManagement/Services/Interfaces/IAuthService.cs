using LeaveOTManagement.DTOs;

namespace LeaveOTManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

        Task<AuthActionResultDto> SendForgotPasswordOtpAsync(ForgotPasswordSendOtpDto request);

        Task<AuthActionResultDto> VerifyForgotPasswordOtpAsync(ForgotPasswordVerifyOtpDto request);

        Task<AuthActionResultDto> ResetForgotPasswordAsync(ForgotPasswordResetDto request);
    }
}