using System;
using System.Collections.Generic;

namespace Cron.Utils.Extentions
{
    public static class DirectoryExtention
    {
        public static TValue GetOrThrow<TKey, TValue>(this Dictionary<TKey, TValue> directory, TKey key, Exception exception)
        {
            if (directory.ContainsKey(key))
            {
                return directory[key];
            }
            throw exception;
        }
    }
}
