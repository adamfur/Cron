using System;

namespace Cron
{
    public class CronScheduler : ICronScheduler
    {
        private enum CronResultion
        {
            Year,
            Month,
            Day,
            Hour,
            Minute,
            Second,
            Millisecond
        };

        private Func<DateTime, bool> _years;
        private Func<DateTime, bool> _seconds;
        private Func<DateTime, bool> _months;
        private Func<DateTime, bool> _hours;
        private Func<DateTime, bool> _minutes;
        private Func<DateTime, bool> _days;
        private Func<DateTime, bool> _dayOfWeek;
        private decimal _decimal;

        public CronScheduler(Func<DateTime, bool> year, Func<DateTime, bool> seconds, Func<DateTime, bool> hours, Func<DateTime, bool> minutes, Func<DateTime, bool> months, Func<DateTime, bool> dayOfMonth, Func<DateTime, bool> dayOfWeek, decimal decimalValue)
        {
            _seconds = seconds;
            _years = year;
            _hours = hours;
            _minutes = minutes;
            _months = months;
            _days = dayOfMonth;
            _dayOfWeek = dayOfWeek;
            _decimal = decimalValue;
        }

        public DateTime Next()
        {
            var now = SystemTime.UtcNow;
            var next = now;
            var prev = next;
            var resolution = CronResultion.Year;

            switch (resolution)
            {
                case CronResultion.Year:
                    if (next.Year > 2099)
                    {
                        return DateTime.MaxValue;
                    }

                    while (!_years(next))
                    {
                        if (next.Year > 2099)
                        {
                            return DateTime.MaxValue;
                        }

                        prev = next;
                        next = next.AsYear().AddYears(1);
                    }
                    goto case CronResultion.Month;
                case CronResultion.Month:
                    while (!_months(next))
                    {
                        prev = next;
                        next = next.AsMonth().AddMonths(1);

                        if (prev.Year != next.Year)
                        {
                            goto case CronResultion.Year;
                        }
                    }
                    goto case CronResultion.Day;
                case CronResultion.Day:
                    while (!_days(next) || !_dayOfWeek(next))
                    {
                        prev = next;
                        next = next.AsDay().AddDays(1);

                        if (prev.Month != next.Month)
                        {
                            goto case CronResultion.Month;
                        }
                    }
                    goto case CronResultion.Hour;
                case CronResultion.Hour:
                    while (!_hours(next))
                    {
                        prev = next;
                        next = next.AsHour().AddHours(1);

                        if (prev.Day != next.Day)
                        {
                            goto case CronResultion.Day;
                        }
                    }
                    goto case CronResultion.Minute;
                case CronResultion.Minute:
                    while (!_minutes(next))
                    {
                        prev = next;
                        next = next.AsMinute().AddMinutes(1);

                        if (prev.Hour != next.Hour)
                        {
                            goto case CronResultion.Hour;
                        }
                    }
                    goto case CronResultion.Second;
                case CronResultion.Second:
                    while (!_seconds(next) || (next.Millisecond != 0 && _decimal == 0m))
                    {
                        prev = next;
                        next = next.AsSecond().AddSeconds(1);

                        if (prev.Minute != next.Minute)
                        {
                            goto case CronResultion.Minute;
                        }
                    }

                    if (_decimal != 0m)
                    {
                        goto case CronResultion.Millisecond;
                    }
                    break;
                case CronResultion.Millisecond:
                    var millisecond = 1000.0;
                    var tick = (int)(millisecond * 0.125);
                    var mili = next.Millisecond;

                    Console.WriteLine($"Tick: {tick}");
                    if (tick == 0)
                    {
                        break;
                    }

                    var result = mili / tick * tick + (mili % tick != 0 ? tick : 0);

                    if (result >= 1000)
                    {
                        next = next.AsSecond().AddSeconds(1);
                        goto case CronResultion.Second;
                    }
                    next = next.AsSecond().AddMilliseconds(result);
                    break;
            }

            Console.WriteLine($"Next Date: {next:yyyy-MM-dd HH:mm:ss.fff} {next.DayOfWeek}, year:{_years(next)}, month:{_months(next)}, days:{_days(next)} hours:{_hours(next)}, minutes:{_minutes(next)}, seconds:{_seconds(next)}");
            return next;
        }
    }
}