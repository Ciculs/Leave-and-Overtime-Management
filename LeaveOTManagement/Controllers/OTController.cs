//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using LeaveOTManagement.Services.Interfaces;
//using LeaveOTManagement.DTOs.OT;

//namespace LeaveOTManagement.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    //[Authorize]
//    public class OTController : ControllerBase
//    {
//        private readonly IOTService _service;

//        public OTController(IOTService service)
//        {
//            _service = service;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(CreateOtRequestDto dto)
//        {
//            var claim = User.FindFirst("UserId");
//            if (claim == null)
//                return Unauthorized();

//            int userId = int.Parse(claim.Value);

//            var id = await _service.CreateOtAsync(userId, dto);

//            return Ok(new { id });
//        }

//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(long id, UpdateOtRequestDto dto)
//        {
//            int userId = int.Parse(User.FindFirst("UserId")!.Value);

//            await _service.UpdateOtAsync(id, userId, dto);

//            return Ok();
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetMyOt([FromQuery] string? status)
//        {
//            int userId = int.Parse(User.FindFirst("UserId")!.Value);

//            var result = await _service.GetMyOtAsync(userId, status);

//            return Ok(result);
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using LeaveOTManagement.Services.Interfaces;
using LeaveOTManagement.DTOs.OT;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTController : ControllerBase
    {
        private readonly IOTService _service;

        public OTController(IOTService service)
        {
            _service = service;
        }

        // TEST MODE: hard-code userId
        private int GetTestUserId() => 1;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOtRequestDto dto)
        {
            var userId = GetTestUserId();

            var id = await _service.CreateOtAsync(userId, dto);

            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateOtRequestDto dto)
        {
            var userId = GetTestUserId();

            await _service.UpdateOtAsync(id, userId, dto);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOt([FromQuery] string? status)
        {
            var userId = GetTestUserId();

            var result = await _service.GetMyOtAsync(userId, status);

            return Ok(result);
        }
    }
}