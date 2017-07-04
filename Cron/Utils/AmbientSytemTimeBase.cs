using System;
using System.Threading;

namespace Cron
{
    public abstract class AmbientSytemTimeBase
    {
        protected static readonly AsyncLocal<Func<DateTime>> AsyncLocal = new AsyncLocal<Func<DateTime>>();
    }
}