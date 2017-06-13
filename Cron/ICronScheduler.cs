using System;

namespace Cron
{
    public interface ICronScheduler
    {
        DateTime Next();
    }
}
