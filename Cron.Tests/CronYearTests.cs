using Xunit;

namespace Cron.Tests
{
    public class CronYearTests : CronTestsBase
    {
        [Theory]
        [InlineData("", "2000-01-01")] // Empty
        [InlineData("*", "2000-01-01, 2001-01-01")] // Wildcard
        [InlineData("2005", "2005-01-01, EOF")] // Value
        [InlineData("2002-2004", "2002-01-01, 2003-01-01, 2004-01-01, EOF")] // Range (1970-2099)
        [InlineData("2002,2003", "2002-01-01, 2003-01-01, EOF")] // Many
        [InlineData("2005/25", "2005-01-01, 2030-01-01, 2055-01-01")] // Increment 
        [InlineData("*/25", "2000-01-01, 2025-01-01")] // Increment 
        [InlineData("/25", "2000-01-01, 2025-01-01")] // Increment 
        public void TestYear(string cron, string targets)
        {
            Impl($"0 0 0 1 1 * {cron}", targets);
        }
    }
}
