using System;

namespace Cron
{
    public static class DayOfWeekExtentions
    {
        public static bool IsWeekend(this DayOfWeek value)
        {
            return value == DayOfWeek.Saturday || value == DayOfWeek.Sunday;
        }
    }
}
