using System.Collections.Generic;

namespace Cron
{
    public static class HashSetExtentions
    {
        public static void AddRange<T>(this ISet<T> value, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                value.Add(item);
            }
        }
    }
}
