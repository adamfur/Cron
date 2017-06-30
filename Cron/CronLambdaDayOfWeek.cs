using System;

namespace Cron
{
    public class CronLambdaDayOfWeek : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return (int) dt.DayOfWeek;
        }
    }            
}
