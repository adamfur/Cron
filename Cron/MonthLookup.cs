using System;
using System.Collections.Generic;

namespace Cron
{
    public class MonthLookup : IMonthLookup
    {
        private Dictionary<DayOfWeek, int> _lastWeekday = new Dictionary<DayOfWeek, int>();
        private CompositeDictionary<DayOfWeek, int, DateTime> _nthWeekday = new CompositeDictionary<DayOfWeek, int, DateTime>();
        private Dictionary<int, int> _closestWeekday = new Dictionary<int, int>();
        private HashSet<int> _map = new HashSet<int>();
        private int _lastWeekdayOfMonth;

        public MonthLookup(DateTime date, int[] weekdays)
        {
            var offset = new Dictionary<DayOfWeek, int>
            {
                [DayOfWeek.Monday] = 0,
                [DayOfWeek.Tuesday] = 0,
                [DayOfWeek.Wednesday] = 0,
                [DayOfWeek.Thursday] = 0,
                [DayOfWeek.Friday] = 0,
                [DayOfWeek.Saturday] = 0,
                [DayOfWeek.Sunday] = 0
            };
            var array = new int[] { 0, -1, 1, -2, 2 };
            
            for (var day = date; day < date.AddMonths(1); day = day.AddDays(1))
            {
                _lastWeekday[day.DayOfWeek] = day.Day;
                ++offset[day.DayOfWeek];
                _nthWeekday[day.DayOfWeek, offset[day.DayOfWeek]] = day;

                if (!day.IsWeekend())
                {
                    _lastWeekdayOfMonth = day.Day;
                }

                foreach (var i in array)
                {
                    var time = day.AddDays(i);

                    if (!time.IsWeekend() && time.Month == day.Month)
                    {
                        _closestWeekday[day.Day] = time.Day;
                        break;
                    }
                }
            }

            foreach (var weekday in weekdays)
            {
                _map.Add(_closestWeekday[weekday]);
            }
        }

        public bool ClosesWeekday(DateTime day)
        {
            return _map.Contains(day.Day);
        }

        public bool LastWeekday(DayOfWeek dayOfWeek, DateTime day)
        {
            return _lastWeekday[dayOfWeek] == day.Day;
        }

        public bool NthWeekday(int nth, DayOfWeek dayOfWeek, DateTime dt)
        {
            return _nthWeekday.ContainsKey(dayOfWeek, nth) ? _nthWeekday[dayOfWeek, nth] == dt : false;
        }

        public bool LastWeekdayOfMonth(DateTime day)
        {
            return day.Day == _lastWeekdayOfMonth;
        }
    }
#if false
    public interface ICronMonth
    {
        bool LastWeekday(DayOfWeek dayOfWeek, DateTime day);
        bool NthWeekday(int nth, DayOfWeek dayOfWeek, DateTime dt);
        bool ClosesWeekday(DateTime day);
        bool LastWeekdayOfMonth(DateTime day);
    }

    public class CronMonth : ICronMonth
    {
        private Dictionary<DayOfWeek, int> _lastWeekday = new Dictionary<DayOfWeek, int>();
        private CompositeDictionary<DayOfWeek, int, int> _nthWeekday = new CompositeDictionary<DayOfWeek, int, int>();
        private int _lastWeekdayOfMonth;
        private Dictionary<int, int> _closestWeekday = new Dictionary<int, int>();

        public CronMonth(Dictionary<DayOfWeek, int> lastWeekday, CompositeDictionary<DayOfWeek, int, int> nthWeekday, int lastWeekdayOfMonth, Dictionary<int, int> closestWeekday)
        {
            _lastWeekday = lastWeekday;
            _nthWeekday = nthWeekday;
            _lastWeekdayOfMonth = lastWeekdayOfMonth;
            _closestWeekday = closestWeekday;
        }

        public bool LastWeekday(DayOfWeek dayOfWeek, DateTime day)
        {
            return _lastWeekday[dayOfWeek] == day.Day;
        }

        public bool NthWeekday(int nth, DayOfWeek dayOfWeek, DateTime dt)
        {
            return _nthWeekday.ContainsKey(dayOfWeek, nth) ? _nthWeekday[dayOfWeek, nth] == dt.Day : false;
        }

        public bool LastWeekdayOfMonth(DateTime day)
        {
            return day.Day == _lastWeekdayOfMonth;
        }

        public bool ClosesWeekday(DateTime day)
        {
            throw new NotImplementedException();
        }
    }

    public class CronMonthFlyweight
    {
        private CompositeDictionary<DayOfWeek, int, ICronMonth> _map = new CompositeDictionary<DayOfWeek, int, ICronMonth>();

        private ICronMonth Build(DayOfWeek dayOfWeek, int daysOfMonth)
        {
            if (_map.ContainsKey(dayOfWeek, daysOfMonth))
            {
                return _map[dayOfWeek, daysOfMonth];
            }

            return _map[dayOfWeek, daysOfMonth] = BuildVariation(dayOfWeek, daysOfMonth);
        }

        public ICronMonth Build(DateTime datetime)
        {
            return Build(datetime.DayOfWeek, datetime.DaysOfMonth());
        }

        private ICronMonth BuildVariation(DayOfWeek startDayOfWeek, int daysOfMonth)
        {
            var lastWeekday = new Dictionary<DayOfWeek, int>();
            var nthWeekday = new CompositeDictionary<DayOfWeek, int, int>();
            var closestWeekday = new Dictionary<int, int>();
            var map = new HashSet<int>();
            int lastWeekdayOfMonth = 0;
            var dayOfWeek = startDayOfWeek;
            var offset = new Dictionary<DayOfWeek, int>
            {
                [DayOfWeek.Monday] = 0,
                [DayOfWeek.Tuesday] = 0,
                [DayOfWeek.Wednesday] = 0,
                [DayOfWeek.Thursday] = 0,
                [DayOfWeek.Friday] = 0,
                [DayOfWeek.Saturday] = 0,
                [DayOfWeek.Sunday] = 0
            };
            var array = new int[] { 0, -1, 1, -2, 2 };

            for (var day = 1; day < daysOfMonth; ++day)
            {
                lastWeekday[dayOfWeek] = day;
                ++offset[dayOfWeek];
                nthWeekday[dayOfWeek, offset[dayOfWeek]] = day;

                if (!dayOfWeek.IsWeekend())
                {
                    lastWeekdayOfMonth = day;
                }

                foreach (var i in array)
                {
                    var time = day + i;

                    if (/*!time.IsWeekend() &&*/ i + day <= daysOfMonth)
                    {
                        closestWeekday[day] = time;
                        break;
                    }
                }
                //dayOfWeek = (DayOfWeek) ((int) dayOfWeek + 1) % ((int) DayOfWeek.Saturday + 1);
            }

            return new CronMonth(lastWeekday, nthWeekday, lastWeekdayOfMonth, closestWeekday);
        }
    }
#endif    
}
