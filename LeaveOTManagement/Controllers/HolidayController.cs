using LeaveOTManagement.Data;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LeaveOTManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public HolidayController(LeaveOTContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Holidays
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return Ok(holiday);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest("Invalid Excel file");

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var dateText = worksheet.Cells[row, 1].Text?.Trim();
                var name = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrEmpty(dateText) || string.IsNullOrEmpty(name))
                    continue;

                var date = DateOnly.Parse(dateText);

                bool exists = await _context.Holidays
                    .AnyAsync(h => h.HolidayDate == date);

                if (!exists)
                {
                    _context.Holidays.Add(new Holiday
                    {
                        HolidayDate = date,
                        Name = name
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Import successful");
        }
    }
}