using System;

namespace Cron
{
    public class MonthLookupFactory : IMonthLookupFactory
    {
        private CompositeDictionary<int, int, IMonthLookup> _lookup = new CompositeDictionary<int, int, IMonthLookup>();

        public IMonthLookup Create(DateTime datetime, int[] array)
        {
            if (_lookup.ContainsKey(datetime.Year, datetime.Month))
            {
                return _lookup[datetime.Year, datetime.Month];
            }

            var lookup = new MonthLookup(datetime.AsMonth(), array);

            _lookup.Clear();
            _lookup[datetime.Year, datetime.Month] = lookup;
            return lookup;
        }
    }
}
