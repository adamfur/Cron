using Xunit;

namespace Cron.Tests
{
    public class CronSecondTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01 00:00:00, 2000-01-01 00:00:01")] // Wildcard
        [InlineData("*", "2000-01-01 00:00:00, 2000-01-01 00:00:01")] // Wildcard
        [InlineData("3", "2000-01-01 00:00:03, 2000-01-01 00:01:03")] // Value
        [InlineData("2-4", "2000-01-01 00:00:02, 2000-01-01 00:00:03, 2000-01-01 00:00:04, 2000-01-01 00:01:02")] // Range (0-59)
        [InlineData("2-4,7", "2000-01-01 00:00:02, 2000-01-01 00:00:03, 2000-01-01 00:00:04, 2000-01-01 00:00:07, 2000-01-01 00:01:02")] // Range (0-59)
        [InlineData("2,4", "2000-01-01 00:00:02, 2000-01-01 00:00:04, 2000-01-01 00:01:02")] // Many
        [InlineData("5/25", "2000-01-01 00:00:05, 2000-01-01 00:00:30, 2000-01-01 00:00:55, 2000-01-01 00:01:05")] // Increment
        [InlineData("*/5", "2000-01-01 00:00:00, 2000-01-01 00:00:05")] // Increment
        [InlineData("/5", "2000-01-01 00:00:00, 2000-01-01 00:00:05")] // Increment
        // [InlineData("/0.125", "2000-01-01 00:00:00.000, 2000-01-01 00:00:00.125, 2000-01-01 00:00:00.250")] // Millisecond resolution
        // [InlineData("5/0.125", "2000-01-01 00:00:05.000, 2000-01-01 00:00:05.125")] // Millisecond resolution
        public void TestSeconds(string cron, string targets)
        {
            Impl($"{cron} * * * * *", targets);
        }
    }
}
