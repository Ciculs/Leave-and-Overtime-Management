using LeaveOTManagement.Data;
using LeaveOTManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

namespace LeaveOTManagement.Controllers
{
    [Route("api/holidays")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public HolidayController(LeaveOTContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL HOLIDAYS
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
        // GET HOLIDAYS BY YEAR (AUTO SYNC)
        // ===============================
        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var holidays = await _context.Holidays
                .Where(h => h.HolidayDate.Year == year)
                .ToListAsync();

            // nếu DB chưa có holiday của năm này
            if (!holidays.Any())
            {
                var client = new HttpClient();

                var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/VN";

                var response = await client.GetStringAsync(url);

                var apiHolidays = JsonSerializer.Deserialize<List<HolidayApiDto>>(response);

                if (apiHolidays != null)
                {
                    foreach (var h in apiHolidays)
                    {
                        var date = DateOnly.Parse(h.date);

                        bool exists = await _context.Holidays
                            .AnyAsync(x => x.HolidayDate == date);

                        if (!exists)
                        {
                            _context.Holidays.Add(new Holiday
                            {
                                HolidayDate = date,
                                Name = h.localName
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                holidays = await _context.Holidays
                    .Where(h => h.HolidayDate.Year == year)
                    .ToListAsync();
            }

            return Ok(holidays);
        }

        // ===============================
        // CREATE HOLIDAY
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Create(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return Ok(holiday);
        }

        // ===============================
        // IMPORT EXCEL
        // ===============================
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

    // ===============================
    // DTO FOR HOLIDAY API
    // ===============================
    public class HolidayApiDto
    {
        public string date { get; set; }
        public string localName { get; set; }
    }
}