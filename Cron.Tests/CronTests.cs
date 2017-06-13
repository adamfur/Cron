using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Cron.Tests
{
    public class CronTests
    {
        // Support */1 = 0/1
        // Support /1 = 0/1
        // Throw on /[0]

        // OK, DICTIONARY
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
        // [InlineData("u/0.125", "2000-01-01 00:00:00.000, 2000-01-01 00:00:00.125, 2000-01-01 00:00:00.250")] // Millisecond resolution
        // [InlineData("u5/0.125", "2000-01-01 00:00:05.000, 2000-01-01 00:00:05.125")] // Millisecond resolution
        public void TestSeconds(string cron, string targets)
        {
            Impl($"{cron} * * * * *", targets);
        }

        // OK, DICTIONARY
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

        // OK, DICTIONARY
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

        // NOT OK
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
        //[InlineData("LW", "2000-01-31, 2000-02-29")] // "last weekday of the month"*.  (NOT IMPLEMENTED)
        //[InlineData("1W", "2000-01-03")] //(NOT IMPLEMENTED)
        //[InlineData("2W", "2000-01-03")] //(NOT IMPLEMENTED)
        //[InlineData("3W", "2000-01-03")] //(NOT IMPLEMENTED)
        //[InlineData("7W", "2000-01-07")] //(NOT IMPLEMENTED)
        //[InlineData("8W", "2000-01-07")] //(NOT IMPLEMENTED)
        // [InlineData("9W", "2000-01-10")] //the nearest weekday to the 15th of the month (same month) (NOT IMPLEMENTED)
        public void TestDayOfMonth(string cron, string targets)
        {
            Impl($"0 0 0 {cron} * *", targets);
        }

        // OK, DICTIONARY
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

        // NOT OK, DICTIONARY
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
        // [InlineData("6#2", "2000-01-14, 2000-02-11")] // 2nd friday of month (NOT IMPLEMENTED)
        // [InlineData("6L", "2000-01-28, 2000-02-25")] // last friday of month (NOT IMPLEMENTED)
        public void TestDayOfWeek(string cron, string targets)
        {
            Impl($"0 0 0 * 1 {cron}", targets);
        }

        // OK, DICTIONARY
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

        [Theory]
        [InlineData("0 0 12 * * ?", "2000-01-01 12:00, 2000-01-02 12:00")]
        [InlineData("0 15 10 ? * *", "2000-01-01 10:15, 2000-01-02 10:15")]
        [InlineData("0 15 10 * * ?", "2000-01-01 10:15, 2000-01-02 10:15")]
        [InlineData("0 15 10 * * ? *", "2000-01-01 10:15, 2000-01-02 10:15")]
        [InlineData("0 15 10 * * ? 2005", "2005-01-01 10:15, 2005-01-02 10:15")]
        [InlineData("0 * 14 * * ?", "2000-01-01 14:00:00")]
        [InlineData("0 0/5 14 * * ?", "2000-01-01 14:00, 2000-01-01 14:05")]
        [InlineData("0 0/5 14,18 * * ?", "2000-01-01 14:00, 2000-01-01 14:05")]
        [InlineData("0 0-5 14 * * ?", "2000-01-01 14:00, 2000-01-01 14:01")]
        [InlineData("0 10,44 14 ? 3 WED", "2000-03-01 14:10, 2000-03-01 14:44")]
        [InlineData("0 15 10 ? * MON-FRI", "2000-01-03 10:15, 2000-01-04 10:15 ")]
        [InlineData("0 15 10 15 * ?", "2000-01-15 10:15, 2000-02-15 10:15")]
        [InlineData("0 0 12 1/5 * ?", "2000-01-01 12:00, 2000-01-06 12:00")]
        [InlineData("0 11 11 11 11 ?", "2000-11-11 11:11, 2001-11-11 11:11")]
        [InlineData("0 15 10 L * ?", "2000-01-31 10:15, 2000-02-29 10:15")]
        [InlineData("0 15 10 L-2 * ?", "2000-01-29 10:15")]
        // .[InlineData("0 15 10 ? * 6L", "2005-01-01")] (NOT IMPLEMENTED)
        // .[InlineData("0 15 10 ? * 6L", "2005-01-01")] (NOT IMPLEMENTED)
        // .[InlineData("0 15 10 ? * 6L 2002-2005", "2005-01-01")] (NOT IMPLEMENTED)
        // .[InlineData("0 15 10 ? * 6#3", "2005-01-01")] (NOT IMPLEMENTED)
        public void QuartzExamplesTests(string cron, string targets)
        {
            Impl(cron, targets);
        }

        private void Impl(string cron, string target)
        {
            var dates = target.Replace(", EOF", "").Split(',').Select(x => DateTime.Parse(x.Trim())); //9999-12-31 23:59:59.9999999
            var parser = new CronParser();
            var scheduler = parser.Parse(cron);
            var time = new DateTime(2000, 01, 01);

            Console.WriteLine("Parsing done!");
            foreach (var date in dates)
            {
                DateTime next;

                using (new AmbientSystemTimeScope(() => time))
                {
                    next = scheduler.Next();
                    Assert.Equal(date, next);
                }
                time = next.AddMilliseconds(1);
            }
        }
    }
}
