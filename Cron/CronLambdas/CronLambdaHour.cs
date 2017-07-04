using System;

namespace Cron
{
    public class CronLambdaHour : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return dt.Hour;
        }
    }      
}
