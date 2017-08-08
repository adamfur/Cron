using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Cron.Tests
{
    public class CronTestsBase
    {
        protected void Impl(string cron, string target)
        {
            var dates = target.Replace("EOF", "9999-12-31 23:59:59.9999999").Split(',').Select(x => DateTime.Parse(x.Trim()));
            var parser = new CronParser(new MonthLookupFactory());
            var scheduler = parser.Parse(cron);
            var time = new DateTime(2000, 01, 01).AddMilliseconds(-1);

            foreach (var date in dates)
            {
                DateTime next;

                using (new AmbientSystemTimeScope(() => time))
                {
                    next = scheduler.Next();
                    Assert.Equal(date, next);
                }

                if (next == DateTime.MaxValue)
                {
                    break;
                }

                time = next.AddMilliseconds(1);
            }
        }
    }
}
