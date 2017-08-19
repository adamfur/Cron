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
            return new DateTime(value.Year, value.Month, value.DaysOfMonth());
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
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0);
        }

        public static bool IsWeekend(this DateTime value)
        {
            return value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;
        }

        public static int DaysOfMonth(this DateTime value)
        {
            switch (value.Month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                case 2:
                    return (value.Year % 4 == 0) ? 29 : 28;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
