using System;

namespace Cron
{
    public interface IMonthLookup
    {
        bool LastWeekday(DayOfWeek dayOfWeek, DateTime day);
        bool NthWeekday(int nth, DayOfWeek dayOfWeek, DateTime dt);
        bool ClosesWeekday(DateTime day);
        bool LastWeekdayOfMonth(DateTime day);
    }
}
