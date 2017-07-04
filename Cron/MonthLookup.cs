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
}
