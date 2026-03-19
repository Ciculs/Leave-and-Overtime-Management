using LeaveOTManagement.Data;
using LeaveOTManagement.DTOs.Leave;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Services
{
    public class LeaveService
    {
        private readonly LeaveOTContext _context;

        public LeaveService(LeaveOTContext context)
        {
            _context = context;
        }

        public async Task<List<TeamCalendarDto>> GetTeamCalendar(int managerId, int year, int month)
        {
            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1);

            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .Where(l =>
                    l.Status == "Approved" &&
                    l.User.ManagerId == managerId &&
                    l.FromDate < end &&
                    l.ToDate >= start
                )
                .Select(l => new TeamCalendarDto
                {
                    EmployeeName = l.User.FullName,
                    LeaveType = l.LeaveType.Name,
                    StartDate = l.FromDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = l.ToDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToListAsync();

            return leaves;
        }
    }
}