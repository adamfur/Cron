using System;

namespace Cron
{
    public class CronLambdaMinute : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return dt.Minute;
        }
    }  
}
