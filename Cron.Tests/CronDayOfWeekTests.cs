using Xunit;

namespace Cron.Tests
{
    public class CronDayOfWeekTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01, 2000-01-02")] // Wildcard
        [InlineData("*", "2000-01-01, 2000-01-02")] // Wildcard
        [InlineData("4", "2000-01-05, 2000-01-12")] // Value
        [InlineData("WED", "2000-01-05, 2000-01-12")] // Value text
        [InlineData("2-4", "2000-01-03, 2000-01-04, 2000-01-05, 2000-01-10")] // Range (1-7)
        [InlineData("MON-WED", "2000-01-03, 2000-01-04, 2000-01-05, 2000-01-10")] // Range text
        [InlineData("2,4", "2000-01-03, 2000-01-05, 2000-01-10")] // Many text
        [InlineData("MON,WED", "2000-01-03, 2000-01-05, 2000-01-10")] // Many text
        [InlineData("SAT", "2000-01-01, 2000-01-08")] // Saturaday
        [InlineData("7", "2000-01-01, 2000-01-08")] // Saturaday
        [InlineData("L", "2000-01-01, 2000-01-08")] // Saturaday
        [InlineData("4#2", "2000-01-12, 2001-01-10")] // 2nd wednesday of month
        [InlineData("WED#2", "2000-01-12, 2001-01-10")] // 2nd wednesday of month
        [InlineData("4L", "2000-01-26, 2001-01-31")] // last wednesday of month (NOT IMPLEMENTED)
        [InlineData("WEDL", "2000-01-26, 2001-01-31")] // last wednesday of month (NOT IMPLEMENTED)
        public void TestDayOfWeek(string cron, string targets)
        {
            Impl($"0 0 0 * 1 {cron} *", targets);
        }
    }
}
