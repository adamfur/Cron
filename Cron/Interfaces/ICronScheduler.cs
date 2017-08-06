using System;

namespace Cron
{
    public interface ICronScheduler
    {
        DateTime Next();
        DateTime Next(DateTime from);
    }
}
