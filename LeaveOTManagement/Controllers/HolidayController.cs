using LeaveOTManagement.Data;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LeaveOTManagement.Controllers
{
    [Route("api/holidays")]
    [ApiController]
    [Authorize]
    public class HolidayController : ControllerBase
    {
        private readonly LeaveOTContext _context;
        private readonly VietnameseHolidayService _vnHolidayService;

        public HolidayController(LeaveOTContext context)
        {
            _context = context;
            _vnHolidayService = new VietnameseHolidayService();
        }

        // ===============================
        // GET ALL
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Holidays
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            return Ok(data);
        }

        // ===============================
        // GET BY YEAR
        // ===============================
        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var holidays = await _context.Holidays
                .Where(h => h.HolidayDate.Year == year)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            return Ok(holidays);
        }

        // ===============================
        // SYNC VIETNAM HOLIDAY
        // ===============================
        [HttpPost("sync/{year}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> SyncByYear(int year)
        {
            var generated = _vnHolidayService.Generate(year);
            int added = 0;

            foreach (var h in generated)
            {
                bool exists = await _context.Holidays
                    .AnyAsync(x => x.HolidayDate == h.HolidayDate);

                if (!exists)
                {
                    _context.Holidays.Add(new Holiday
                    {
                        HolidayDate = h.HolidayDate,
                        Name = h.Name,
                        IsDayOff = true,
                        IsLeaveBlocked = false,
                        Source = "API"
                    });

                    added++;
                }
            }

            await _context.SaveChangesAsync();

            var holidays = await _context.Holidays
                .Where(h => h.HolidayDate.Year == year)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            return Ok(new
            {
                message = $"Generated {added} holidays for {year}",
                data = holidays
            });
        }

        // ===============================
        // CREATE
        // ===============================
        [HttpPost]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> Create([FromBody] Holiday holiday)
        {
            if (holiday == null)
                return BadRequest("Invalid data.");

            if (string.IsNullOrWhiteSpace(holiday.Name))
                return BadRequest("Holiday name is required.");

            bool exists = await _context.Holidays
                .AnyAsync(h => h.HolidayDate == holiday.HolidayDate);

            if (exists)
                return BadRequest("This date already exists.");

            holiday.Source ??= "Manual";

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return Ok(holiday);
        }

        // ===============================
        // UPDATE
        // ===============================
        [HttpPut("{id}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Holiday model)
        {
            var holiday = await _context.Holidays.FindAsync(id);

            if (holiday == null)
                return NotFound("Holiday not found.");

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Holiday name is required.");

            bool duplicate = await _context.Holidays
                .AnyAsync(h => h.Id != id && h.HolidayDate == model.HolidayDate);

            if (duplicate)
                return BadRequest("Another holiday already uses this date.");

            holiday.HolidayDate = model.HolidayDate;
            holiday.Name = model.Name;
            holiday.IsDayOff = model.IsDayOff;
            holiday.IsLeaveBlocked = model.IsLeaveBlocked;
            holiday.Source = string.IsNullOrWhiteSpace(model.Source)
                ? holiday.Source
                : model.Source;
            holiday.Note = model.Note;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Holiday updated successfully." });
        }

        // ===============================
        // DELETE
        // ===============================
        [HttpDelete("{id}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);

            if (holiday == null)
                return NotFound("Holiday not found.");

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Holiday deleted successfully." });
        }

        // ===============================
        // IMPORT EXCEL
        // ===============================
        [HttpPost("import")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest("Invalid Excel file.");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int added = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var dateText = worksheet.Cells[row, 1].Text?.Trim();
                var name = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrWhiteSpace(dateText) || string.IsNullOrWhiteSpace(name))
                    continue;

                if (!DateOnly.TryParse(dateText, out var date))
                    continue;

                bool exists = await _context.Holidays
                    .AnyAsync(h => h.HolidayDate == date);

                if (!exists)
                {
                    _context.Holidays.Add(new Holiday
                    {
                        HolidayDate = date,
                        Name = name,
                        IsDayOff = true,
                        IsLeaveBlocked = false,
                        Source = "Import"
                    });

                    added++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Import successful. Added {added} holidays."
            });
        }
    }
}