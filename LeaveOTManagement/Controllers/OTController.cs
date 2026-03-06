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

        [HttpPost]
        public async Task<IActionResult> Create(CreateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var id = await _service.CreateOtAsync(userId, dto);

            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateOtRequestDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            await _service.UpdateOtAsync(id, userId, dto);

            return Ok();
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
                return NotFound();

            return Ok(result);
        }
    }
}