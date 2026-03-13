using LeaveOTManagement.DTOs.OT;

namespace LeaveOTManagement.Services.Interfaces
{
    public interface IOTService
    {
        Task<long> CreateOtAsync(int userId, CreateOtRequestDto dto);
        Task UpdateOtAsync(long id, int userId, UpdateOtRequestDto dto);
        Task<List<OtResponseDto>> GetMyOtAsync(int userId, string? status);
        Task<OtResponseDto?> GetOtByIdAsync(long id, int userId);
        Task<List<OtResponseDto>> GetPendingApprovalsAsync(int approverId);
        Task ManagerApproveOtAsync(long requestId, int approverId);
        Task HrApproveOtAsync(long requestId, int approverId);
        Task RejectOtAsync(long requestId, int approverId, string reason);
    }
}
