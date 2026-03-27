using System.Security.Claims;
using LeaveOTManagement.DTOs.OT;
using LeaveOTManagement.Services;
using LeaveOTManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            try
            {
                var id = await _service.CreateOtAsync(userId, dto);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            try
            {
                await _service.UpdateOtAsync(id, userId, dto);
                return Ok(new { message = "OT request updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOt([FromQuery] string? status)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetMyOtAsync(userId, status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetOtByIdAsync(id, userId);

            if (result == null)
                return NotFound(new { message = "OT request not found." });

            return Ok(result);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetPendingApprovalsAsync(userId);
            return Ok(result);
        }

        [HttpPut("{id}/manager-approve")]
        public async Task<IActionResult> ManagerApprove(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            try
            {
                await _service.ManagerApproveOtAsync(id, userId);
                return Ok(new { message = "Manager approved. Sent to HR." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}/hr-approve")]
        public async Task<IActionResult> HrApprove(long id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            try
            {
                await _service.HrApproveOtAsync(id, userId);
                return Ok(new { message = "HR approved OT request." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    innerMost = ex.InnerException?.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    innerMost = ex.InnerException?.InnerException?.Message
                });
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(long id, [FromBody] RejectOtDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Reject reason is required." });

            try
            {
                await _service.RejectOtAsync(id, userId, dto.Reason);
                return Ok(new { message = "OT request rejected." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("hr-pending")]
        public async Task<IActionResult> GetHrPending()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var result = await _service.GetPendingApprovalsAsync(userId);
            return Ok(result);
        }

        [HttpGet("team-calendar")]
        public async Task<IActionResult> GetTeamCalendar([FromQuery] int year, [FromQuery] int month)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            try
            {
                var result = await _service.GetTeamOtCalendarAsync(userId, year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }
    }
}