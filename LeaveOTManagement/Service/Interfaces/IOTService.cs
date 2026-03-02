using LeaveOTManagement.DTOs.OT;

namespace LeaveOTManagement.Services.Interfaces
{
    public interface IOTService
    {
        Task<long> CreateOtAsync(int userId, CreateOtRequestDto dto);
        Task UpdateOtAsync(long id, int userId, UpdateOtRequestDto dto);
        Task<List<OtResponseDto>> GetMyOtAsync(int userId, string? status);
    }
}
