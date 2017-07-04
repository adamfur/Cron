using System;

namespace Cron
{
    public interface IMonthLookupFactory
    {
        IMonthLookup Create(DateTime datetime, int[] array);
    }
}
