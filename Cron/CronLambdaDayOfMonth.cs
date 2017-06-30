using System;
using System.Collections.Generic;
using System.Linq;

namespace Cron
{
    public class CronLambdaDayOfMonth : CronLambda
    {
        private HashSet<int> _days = new HashSet<int>();

        protected override decimal Property(DateTime dt)
        {
            return dt.Day;
        }

        public void AddClosestWeekday(int day)
        {
            _days.Add(day);
            _base = false;
        }

        public int[] Weekdays()
        {
            return _days.ToArray();
        }
    }    
}
