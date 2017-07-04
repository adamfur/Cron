using System;

namespace Cron
{
    public class CronLambdaMonth : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return dt.Month;
        }
    }            
}
