using ClosedXML.Excel;
using LeaveOTManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR")]
    public class PayrollController : ControllerBase
    {
        private readonly LeaveOTContext _context;

        public PayrollController(LeaveOTContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayroll()
        {
            var data = await _context.PayrollLogs
                .Include(p => p.User)
                .OrderByDescending(p => p.WorkDate)
                .Select(p => new
                {
                    p.Id,
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    p.WorkDate,
                    p.Hours,
                    p.RateMultiplier,
                    TotalPay = p.Hours * p.RateMultiplier
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportPayroll()
        {
            var data = await _context.PayrollLogs
                .Include(p => p.User)
                .Include(p => p.Otrequest)
                .OrderByDescending(p => p.WorkDate)
                .Select(p => new
                {
                    PayrollId = p.Id,
                    UserId = p.UserId,
                    EmployeeName = p.User.FullName,
                    OTRequestId = p.OTRequestId,
                    WorkDate = p.WorkDate,
                    Hours = p.Hours,
                    RateMultiplier = p.RateMultiplier,
                    TotalPay = p.Hours * p.RateMultiplier,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Payroll");

            // Title
            worksheet.Cell(1, 1).Value = "PAYROLL EXPORT";
            worksheet.Range(1, 1, 1, 8).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Export time
            worksheet.Cell(2, 1).Value = $"Exported at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Range(2, 1, 2, 8).Merge();
            worksheet.Cell(2, 1).Style.Font.Italic = true;
            worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.DimGray;

            // Header
            int headerRow = 4;
            worksheet.Cell(headerRow, 1).Value = "Payroll ID";
            worksheet.Cell(headerRow, 2).Value = "User ID";
            worksheet.Cell(headerRow, 3).Value = "Employee Name";
            worksheet.Cell(headerRow, 4).Value = "OT Request ID";
            worksheet.Cell(headerRow, 5).Value = "Work Date";
            worksheet.Cell(headerRow, 6).Value = "Hours";
            worksheet.Cell(headerRow, 7).Value = "Rate Multiplier";
            worksheet.Cell(headerRow, 8).Value = "Total Pay";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4318FF");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Data
            int row = headerRow + 1;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.PayrollId;
                worksheet.Cell(row, 2).Value = item.UserId;
                worksheet.Cell(row, 3).Value = item.EmployeeName;
                worksheet.Cell(row, 4).Value = item.OTRequestId;
                worksheet.Cell(row, 5).Value = item.WorkDate;
                worksheet.Cell(row, 6).Value = item.Hours;
                worksheet.Cell(row, 7).Value = item.RateMultiplier;
                worksheet.Cell(row, 8).Value = item.TotalPay;

                worksheet.Cell(row, 5).Style.DateFormat.Format = "yyyy-MM-dd";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "0.00";
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "0.00";
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "0.00";

                row++;
            }

            // Total row
            worksheet.Cell(row, 1).Value = "TOTAL";
            worksheet.Range(row, 1, row, 7).Merge();
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, 8).FormulaA1 = $"SUM(H{headerRow + 1}:H{row - 1})";
            worksheet.Cell(row, 8).Style.Font.Bold = true;
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "0.00";
            worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9ECF7");

            // Borders
            var usedRange = worksheet.Range(headerRow, 1, row, 8);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Auto fit
            worksheet.Columns().AdjustToContents();

            // Freeze header
            worksheet.SheetView.FreezeRows(headerRow);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Payroll_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}