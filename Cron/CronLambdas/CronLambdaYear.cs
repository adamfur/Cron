using System;

namespace Cron
{
    public class CronLambdaYear : CronLambda
    {
        protected override decimal Property(DateTime dt)
        {
            return dt.Year;
        }
    }
}
