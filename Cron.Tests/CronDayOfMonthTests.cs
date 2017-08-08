using Xunit;

namespace Cron.Tests
{
    public class CronDayOfMonthTests : CronTestsBase
    {
        [Theory]
        [InlineData("?", "2000-01-01, 2000-01-02")] // Wildcard
        [InlineData("*", "2000-01-01, 2000-01-02")] // Wildcard
        [InlineData("3", "2000-01-03, 2000-02-03")] // Value
        [InlineData("2-4", "2000-01-02, 2000-01-03, 2000-01-04, 2000-02-02")] // Range
        [InlineData("2,3", "2000-01-02, 2000-01-03, 2000-02-02")] // Many
        [InlineData("8/10", "2000-01-08, 2000-01-18, 2000-01-28, 2000-02-08")] // Increment
        [InlineData("W", "2000-01-03, 2000-01-04, 2000-01-05, 2000-01-06, 2000-01-07, 2000-01-10")] // Week
        [InlineData("W,9", "2000-01-03, 2000-01-04, 2000-01-05, 2000-01-06, 2000-01-07, 2000-01-09, 2000-01-10")] // Week
        [InlineData("L", "2000-01-31, 2000-02-29")] // Last day of month
        [InlineData("L-1", "2000-01-30, 2000-02-28")] // Last day of month - 1 (not imp)
        [InlineData("*/10", "2000-01-01, 2000-01-11")] // Increment
        [InlineData("/10", "2000-01-01, 2000-01-11")] // Increment
        [InlineData("LW", "2000-01-31, 2000-02-29")] // "last weekday of the month"*.  (NOT IMPLEMENTED)
        [InlineData("1W", "2000-01-03")]
        [InlineData("2W", "2000-01-03")]
        [InlineData("3W", "2000-01-03")]
        [InlineData("7W", "2000-01-07")]
        [InlineData("8W", "2000-01-07")]
        [InlineData("9W", "2000-01-10")] //the nearest weekday to the 15th of the month (same month)
        public void TestDayOfMonth(string cron, string targets)
        {
            Impl($"0 0 0 {cron} * *", targets);
        }
    }
}
