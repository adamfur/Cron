using Xunit;

namespace Cron.Tests
{
    public class CronMonthTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01, 2000-02-01")] // Wildcard
        [InlineData("*", "2000-01-01, 2000-02-01")] // Wildcard
        [InlineData("2", "2000-02-01, 2001-02-01")] // Value
        [InlineData("FEB", "2000-02-01, 2001-02-01")] // Value text
        [InlineData("2-4", "2000-02-01, 2000-03-01, 2000-04-01, 2001-02-01")] // Range (1-12)
        [InlineData("FEB-APR", "2000-02-01, 2000-03-01, 2000-04-01, 2001-02-01")] // Range text
        [InlineData("2,3", "2000-02-01, 2000-03-01, 2001-02-01")] // Many
        [InlineData("FEB,MAR", "2000-02-01, 2000-03-01, 2001-02-01")] // Many text
        [InlineData("2/4", "2000-02-01, 2000-06-01, 2000-10-01, 2001-02-01")] // Increment
        [InlineData("FEB/4", "2000-02-01, 2000-06-01, 2000-10-01, 2001-02-01")] // Increment text
        [InlineData("*/2", "2000-01-01, 2000-03-01")] // Increment text
        [InlineData("/2", "2000-01-01, 2000-03-01")] // Increment text
        public void TestMonth(string cron, string targets)
        {
            Impl($"0 0 0 1 {cron} *", targets);
        }
    }
}
