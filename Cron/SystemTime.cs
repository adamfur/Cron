using System;

namespace Cron
{
    public class SystemTime : AmbientSytemTimeBase
    {
        public static DateTime UtcNow
        {
            get
            {
                var asyncLocal = AmbientSytemTimeBase.AsyncLocal;

                if (asyncLocal.Value != null)
                {
                    return asyncLocal.Value();
                }
                return DateTime.UtcNow;
            }
        }

        public static DateTime UtcToday => UtcNow.Date;
    }
}