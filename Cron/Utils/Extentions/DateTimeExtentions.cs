using System;

namespace Cron
{
    public static class DateTimeExtentions
    {
        public static DateTime AsYear(this DateTime value)
        {
            return new DateTime(value.Year, 1, 1);
        }

        public static DateTime AsMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static DateTime LastOfMonth(this DateTime value)
        {
            return value.AsMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime AsDay(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day);
        }

        public static DateTime AsSecond(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
        }

        public static DateTime AsMinute(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        public static DateTime AsHour(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        public static bool IsWeekend(this DateTime value)
        {
            return value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
