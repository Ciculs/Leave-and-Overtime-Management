using System.Globalization;
using LeaveOTManagement.Models.Entities;

public class VietnameseHolidayService
{
    private readonly ChineseLunisolarCalendar _lunar = new();

    public List<Holiday> Generate(int year)
    {
        var holidays = new List<Holiday>();

        // ===== FIXED DATES =====
        holidays.AddRange(new[]
        {
            Create(year, 1, 1, "New Year", true),
            Create(year, 4, 30, "Reunification Day", true),
            Create(year, 5, 1, "Labor Day", true),
            Create(year, 9, 2, "National Day", true),
            Create(year, 10, 20, "Vietnam Women's Day")
        });

        // ===== TET (ÂM LỊCH) =====
        var tet = GetSolarDate(year, 1, 1);

        for (int i = 0; i < 5; i++)
        {
            holidays.Add(new Holiday
            {
                HolidayDate = tet.AddDays(i),
                Name = "Tet Holiday",
                IsDayOff = true,
                Source = "System"
            });
        }

        // ===== MID AUTUMN =====
        var midAutumn = GetSolarDate(year, 8, 15);

        holidays.Add(new Holiday
        {
            HolidayDate = midAutumn,
            Name = "Mid-Autumn Festival",
            Source = "System"
        });

        return holidays;
    }

    private Holiday Create(int year, int month, int day, string name, bool isDayOff = false)
    {
        return new Holiday
        {
            HolidayDate = new DateOnly(year, month, day),
            Name = name,
            IsDayOff = isDayOff,
            Source = "System"
        };
    }

    private DateOnly GetSolarDate(int year, int lunarMonth, int lunarDay)
    {
        var date = _lunar.ToDateTime(year, lunarMonth, lunarDay, 0, 0, 0, 0);
        return DateOnly.FromDateTime(date);
    }
}