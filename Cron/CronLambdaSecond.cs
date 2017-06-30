using System;

namespace Cron
{
    public class CronLambdaSecond : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return dt.Second;
        }
    }    
}
