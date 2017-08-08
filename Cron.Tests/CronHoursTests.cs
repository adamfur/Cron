using Xunit;

namespace Cron.Tests
{
    public class CronHoursTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01 00:00:00, 2000-01-01 01:00:00")] // Wildcard
        [InlineData("*", "2000-01-01 00:00:00, 2000-01-01 01:00:00")] // Wildcard
        [InlineData("3", "2000-01-01 03:00:00, 2000-01-02 03:00:00")] // Value
        [InlineData("2-4", "2000-01-01 02:00:00, 2000-01-01 03:00:00, 2000-01-01 04:00:00, 2000-01-02 02:00:0")] // Range (0-23)
        [InlineData("2,3", "2000-01-01 02:00:00, 2000-01-01 03:00:00, 2000-01-02 02:00:00")] // Many
        [InlineData("4/8", "2000-01-01 04:00:00, 2000-01-01 12:00:00, 2000-01-01 20:00:00, 2000-01-02 04:00:00")] // Increment
        [InlineData("*/5", "2000-01-01 00:00:00, 2000-01-01 05:00:00")] // Increment
        [InlineData("/5", "2000-01-01 00:00:00, 2000-01-01 05:00:00")] // Increment        
        public void TestHour(string cron, string targets)
        {
            Impl($"0 0 {cron} * * *", targets);
        }
    }
}
