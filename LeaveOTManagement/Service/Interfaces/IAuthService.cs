using LeaveOTManagement.DTOs;

namespace LeaveOTManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    }
}