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

        public async Task<List<TeamCalendarDto>> GetTeamCalendar(int managerId)
        {
            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .Where(l =>
                    l.Status == "Approved" &&
                    l.User.ManagerId == managerId
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