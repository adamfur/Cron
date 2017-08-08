using Xunit;

namespace Cron.Tests
{
    public class CronMinutesTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01 00:00:00, 2000-01-01 00:01:00")] // Wildcard
        [InlineData("*", "2000-01-01 00:00:00, 2000-01-01 00:01:00")] // Wildcard
        [InlineData("3", "2000-01-01 00:03:00, 2000-01-01 01:03:00")] // Value
        [InlineData("2-4", "2000-01-01 00:02:00, 2000-01-01 00:03:00, 2000-01-01 00:04:00, 2000-01-01 01:02:0")] // Range (0-59)
        [InlineData("2,3", "2000-01-01 00:02:00, 2000-01-01 00:03:00, 2000-01-01 01:02:00")] // Many
        [InlineData("5/25", "2000-01-01 00:05:00, 2000-01-01 00:30:00, 2000-01-01 00:55:00, 2000-01-01 01:05:00")] // Increment
        [InlineData("*/5", "2000-01-01 00:00:00, 2000-01-01 00:05:00")] // Increment
        [InlineData("/5", "2000-01-01 00:00:00, 2000-01-01 00:05:00")] // Increment        
        public void TestMinute(string cron, string targets)
        {
            Impl($"0 {cron} * * * *", targets);
        }
    }
}
