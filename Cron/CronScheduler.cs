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

        public CronScheduler(Func<DateTime, bool> year, Func<DateTime, bool> seconds, Func<DateTime, bool> hours, Func<DateTime, bool> minutes, Func<DateTime, bool> months, Func<DateTime, bool> dayOfMonth, Func<DateTime, bool> dayOfWeek)
        {
            _seconds = seconds;
            _years = year;
            _hours = hours;
            _minutes = minutes;
            _months = months;
            _days = dayOfMonth;
            _dayOfWeek = dayOfWeek;
        }

        public DateTime Next()
        {
            return Next(SystemTime.UtcNow);
        }

        // public DateTime Next2(DateTime now)
        // {
        //     var list = new Action[]
        //     {
        //         () => Year(),
        //         () => Month(),
        //         () => Day(),
        //         () => Hour(),
        //         () => Minute(),
        //         () => Second()
        //     };
        //     var next = now;

        //     for (var i = 0; i < list.Length; ++i)
        //     {
        //         var action = list[i];


        //     }
        // }

        // private void Year()
        // {
        // }

        // private void Month()
        // {
        // }

        // private void Day()
        // {
        // }

        // private void Hour()
        // {
        // }

        // private void Minute()
        // {
        // }

        // private void Second()
        // {
        // }

        public DateTime Next(DateTime now)
        {
            var next = now;
            var prev = next;
            var resolution = CronResultion.Year;

            // BUGGY, FIX
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
                    while (!_seconds(next) || next == now)
                    {
                        prev = next;
                        next = next.AsSecond().AddSeconds(1);

                        if (prev.Minute != next.Minute)
                        {
                            goto case CronResultion.Minute;
                        }
                    }

                    goto case CronResultion.Millisecond;
                case CronResultion.Millisecond:
                    //     var millisecond = 1000.0;
                    //     var tick = (int)(millisecond * 0.125);
                    //     var mili = next.Millisecond;

                    //     Console.WriteLine($"Tick: {tick}");
                    //     if (tick == 0)
                    //     {
                    //         break;
                    //     }

                    //     var result = mili / tick * tick + (mili % tick != 0 ? tick : 0);

                    //     if (result >= 1000)
                    //     {
                    //         next = next.AsSecond().AddSeconds(1);
                    //         goto case CronResultion.Second;
                    //     }
                    //     next = next.AsSecond().AddMilliseconds(result);
                    break;
            }

            return next;
        }
    }
}
