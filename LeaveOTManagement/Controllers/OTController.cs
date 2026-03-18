using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveOTManagement.Services.Interfaces;
using LeaveOTManagement.DTOs.OT;
using System.Security.Claims;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OTController : ControllerBase
    {
        private readonly IOTService _service;

        public OTController(IOTService service)
        {
            _service = service;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return false;

            return int.TryParse(claim.Value, out userId);
        }

        // CREATE OT
        [HttpPost]
        public async Task<IActionResult> Create(CreateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var id = await _service.CreateOtAsync(userId, dto);

            return Ok(new { id });
        }

        // UPDATE OT
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            await _service.UpdateOtAsync(id, userId, dto);

            return Ok();
        }

        // GET MY OT
        [HttpGet]
        public async Task<IActionResult> GetMyOt([FromQuery] string? status)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetMyOtAsync(userId, status);

            return Ok(result);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetOtByIdAsync(id, userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET OT PENDING FOR APPROVAL (MANAGER OR HR)
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetPendingApprovalsAsync(userId);

            return Ok(result);
        }

        // MANAGER APPROVE
        [HttpPut("{id}/manager-approve")]
        public async Task<IActionResult> ManagerApprove(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            await _service.ManagerApproveOtAsync(id, userId);

            return Ok(new { message = "Manager approved. Sent to HR." });
        }

        // HR APPROVE
        [HttpPut("{id}/hr-approve")]
        public async Task<IActionResult> HrApprove(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            await _service.HrApproveOtAsync(id, userId);

            return Ok(new { message = "HR approved OT request." });
        }

        // REJECT (MANAGER OR HR)
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(long id, [FromBody] RejectOtDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("Reject reason is required.");

            await _service.RejectOtAsync(id, userId, dto.Reason);

            return Ok(new { message = "OT request rejected." });
        }

        [HttpGet("hr-pending")]
        public async Task<IActionResult> GetHrPending()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetPendingApprovalsAsync(userId);

            return Ok(result);
        }
    }
}