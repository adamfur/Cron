using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cron
{
    public class CronParser : CronParent
    {
        private CronLambda _year = new CronLambdaYear();
        private CronLambda _seconds = new CronLambdaSecond();
        private CronLambda _hours = new CronLambdaHour();
        private CronLambda _minutes = new CronLambdaMinute();
        private CronLambda _months = new CronLambdaMonth();
        private CronLambdaDayOfMonth _dayOfMonth = new CronLambdaDayOfMonth();
        private CronLambda _dayOfWeek = new CronLambdaDayOfWeek();
        private IMonthLookupFactory _factory;
        private string _context;

        public CronParser(IMonthLookupFactory factory)
        {
            _factory = factory;
        }

        public override ICronScheduler Parse(string cron)
        {
            Prepare(cron);
            Parse();
            return new CronScheduler(dt => _year.Allowed(dt), dt => _seconds.Allowed(dt), dt => _hours.Allowed(dt), dt => _minutes.Allowed(dt), dt => _months.Allowed(dt), dt => _dayOfMonth.Allowed(dt), dt => _dayOfWeek.Allowed(dt), 0);
        }

        private void Parse()
        {
            ParseSeconds();
            ParseMinutes();
            ParseHours();
            ParseDayOfMonth();
            ParseMonth();
            ParseDayOfWeek();
            ParseYear(); // optional
        }

        private bool HasFractions(decimal value)
        {
            return value % 1m != 0m;
        }

        private string ReadWord()
        {
            var builder = new StringBuilder();
            
            for (int i = 0; IsAlpha() && i < 3; ++i)
            {
                builder.Append(Symbol);
                NextSymbol();
            }

            return builder.ToString();
        }

        private decimal ReadInteger()
        {
            if (Accept('/'))
            {
                return 0m;
            }

            if (Accept('*'))
            {
                NextSymbol();
                return 0m;
            }

            var builder = new StringBuilder();

            while (IsDigit())
            {
                builder.Append(Symbol);
                NextSymbol();
            }

            return Decimal.Parse(builder.ToString());
        }

        private bool ParseDayOfWeek(CronLambda set, Limit limit)
        {
            if (!IsAlpha() && !IsDigit())
            {
                return false;
            }

            var start = (int)ReadDayOfWeek();

            if (Accept('/'))
            {
                NextSymbol();
                var stride = (int)ReadDayOfWeek();

                for (var i = start; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
            }
            else if (Accept('#'))
            {
                NextSymbol();
                var nth = (int)ReadInteger();

                set.AddLambda(dt =>
                {
                    var dow = (DayOfWeek)start;
                    var result = _factory.Create(dt, new int[0]).NthWeekday(nth, dow, dt);
                    return result;
                });
            }
            else if (Accept('-'))
            {
                NextSymbol();
                var end = (int)ReadDayOfWeek();

                for (var i = start; i <= end; ++i)
                {
                    set.Add(i);
                }
            }
            else if (Accept('L'))
            {
                NextSymbol();
                set.AddLambda(dt =>
                {
                    var dow = (DayOfWeek)start;
                    return _factory.Create(dt, new int[0]).LastWeekday(dow, dt);
                });
            }
            else
            {
                set.Add(start);
            }
            return true;
        }

        private decimal ReadMonth()
        {
            if (Accept('/'))
            {
                return 1;
            }

            if (Accept('*'))
            {
                NextSymbol();
                return 1;
            }

            if (IsAlpha())
            {
                return _monthTranslator[ReadWord()];
            }
            else if (IsDigit())
            {
                return ReadInteger();
            }
            throw new CronException("ReadMonth");
        }

        private decimal ReadDayOfWeek()
        {
            if (Accept('/'))
            {
                return 0;
            }

            if (Accept('*'))
            {
                NextSymbol();
                return 0;
            }

            if (IsAlpha())
            {
                var word = ReadWord();

                Console.WriteLine($"ABC: {word} -> {(DayOfWeek)_dayTextTranslator[word]}");
                return (int)_dayTextTranslator[word];
            }
            else if (IsDigit())
            {
                var hej = (int)ReadInteger();
                var value = (int)_dayNumberTranslator[hej];

                Console.WriteLine($"ABC: {hej} -> {(DayOfWeek) value}");
                return value;
            }
            throw new CronException($"ReadDayOfWeek: [{Symbol}]");
        }

        private bool ParseLast(CronLambdaDayOfMonth set, Limit limit, Func<decimal> readFunc, CronLambda span)
        {
            if (!Accept('L'))
            {
                return false;
            }

            var minusOffset = 0;

            NextSymbol();
            if (Accept('W'))
            {
                NextSymbol();
                set.AddLambda(dt => _factory.Create(dt, set.Weekdays()).LastWeekdayOfMonth(dt));
                return true;
            }
            else if (Accept('-'))
            {
                NextSymbol();
                minusOffset = (int)ReadInteger();
            }

            span.AddLambda((dt) => dt == dt.LastOfMonth().AddDays(-minusOffset));
            return true;
        }

        private bool ParseWeek(CronLambda set, Limit limit, CronLambda span)
        {
            if (!Accept('W'))
            {
                return false;
            }

            NextSymbol();

            span.AddLambda((dt) => (dt.DayOfWeek != DayOfWeek.Sunday && dt.DayOfWeek != DayOfWeek.Saturday));
            return true;
        }

        private void ParseYear()
        {
            SkipSpaces();
            SetContext("ParseYear");

            var limit = new Limit(1900, 2099);

            if (IsEof())
            {
                return;
            }

            ParseBlocks(
                () => ParseWildcard(_year, limit, ReadInteger),
                () => ParseDigit2(_year, limit, ReadInteger),
                () => ParseSlash(_year, limit, ReadInteger)
            );

            Expect(IsEof(), "More data...");
        }        

        private void ParseMonth()
        {
            SkipSpaces();
            SetContext("ParseMonth");

            var limit = new Limit(1, 12);

            ParseBlocks(
                () => ParseWildcard(_months, limit, ReadMonth),
                () => ParseDigit2(_months, limit, ReadMonth),
                () => ParseSlash(_months, limit, ReadMonth)
            );
        }        

        private void ParseDayOfWeek()
        {
            SkipSpaces();
            SetContext("ParseDayOfWeek");

            var limit = new Limit(0, 9);

            ParseBlocks(
                () => ParseSimpleWildcard(_dayOfWeek, limit),
                () => ParseDayOfWeek(_dayOfWeek, limit)
            );
        }        

        private void ParseDayOfMonth()
        {
            var limit = new Limit(1, 31);

            SkipSpaces();
            SetContext("ParseDayOfMonth");
            ParseBlocks(
                () => ParseWildcard(_dayOfMonth, limit, ReadMonth),
                () => ParseDigitWithWeek(_dayOfMonth, limit, ReadMonth),
                () => ParseSlash(_dayOfMonth, limit, ReadMonth),
                () => ParseWeek(_dayOfMonth, limit, _dayOfMonth),
                () => ParseLast(_dayOfMonth, limit, ReadMonth, _dayOfMonth)
            );
        }

        private void ParseHours()
        {
            var limit = new Limit(0, 23);

            SkipSpaces();
            SetContext("ParseHours");
            ParseBlocks(
                () => ParseWildcard(_hours, limit, ReadInteger),
                () => ParseDigit(_hours, limit, ReadInteger),
                () => ParseSlash(_hours, limit, ReadInteger)
            );
        }

        private void ParseMinutes()
        {
            var limit = new Limit(0, 59);

            SkipSpaces();
            SetContext("ParseMinutes");
            ParseBlocks(
                () => ParseWildcard(_minutes, limit, ReadInteger),
                () => ParseDigit(_minutes, limit, ReadInteger),
                () => ParseSlash(_minutes, limit, ReadInteger)
            );
        }

        private void ParseSeconds()
        {
            var limit = new Limit(0, 59);

            SkipSpaces();
            SetContext("ParseSeconds");
            ParseBlocks(
                () => ParseWildcard(_seconds, limit, ReadInteger),
                () => ParseDigit2(_seconds, limit, ReadInteger),
                () => ParseSlash(_seconds, limit, ReadInteger)
            );
        }

        private bool ParseDigit2(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsDigit() && !IsAlpha())
            {
                return false;
            }

            var start = (int)readFunc();
            set.Add(start);

            if (Accept('/'))
            {
                NextSymbol();
                var stride = (int)readFunc();

                for (var i = start; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
            }
            else if (Accept('-'))
            {
                NextSymbol();
                var end = (int)readFunc();

                for (var i = start; i <= end; ++i)
                {
                    set.Add(i);
                }
            }
            else if (Accept('W'))
            {
                NextSymbol();

            }
            return true;
        }

        private bool ParseDigitWithWeek(CronLambdaDayOfMonth set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsDigit())
            {
                return false;
            }

            var start = (int)readFunc();

            if (Accept('/'))
            {
                set.Add(start);
                NextSymbol();
                var stride = (int)readFunc();

                for (var i = start; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
            }
            else if (Accept('W'))
            {
                NextSymbol();
                set.AddClosestWeekday(start);

                set.AddLambda(dt =>
                {
                    //Console.WriteLine($":: {dt:yyyy-MM-dd} {_factory.Create(dt, set.Weekdays()).ClosesWeekday(dt)}");
                    return _factory.Create(dt, set.Weekdays()).ClosesWeekday(dt);
                });
            }
            else if (Accept('-'))
            {
                set.Add(start);
                NextSymbol();
                var end = (int)readFunc();

                for (var i = start; i <= end; ++i)
                {
                    set.Add(i);
                }
            }
            else
            {
                set.Add(start);
            }
            return true;
        }

        private bool ParseDigit(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsDigit() && !IsAlpha())
            {
                return false;
            }

            var start = (int)readFunc();
            set.Add(start);

            if (Accept('/'))
            {
                NextSymbol();
                var stride = (int)readFunc();

                for (var i = start; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
            }
            else if (Accept('-'))
            {
                NextSymbol();
                var end = (int)readFunc();

                for (var i = start; i <= end; ++i)
                {
                    set.Add(i);
                }
            }
            else if (IsWhiteSpace())
            {
            }
            return true;
        }

        private bool ParseSlash(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsSlash())
            {
                return false;
            }

            NextSymbol();
            var stride = (int)ReadInteger();

            for (var i = limit.Min; i <= limit.Max; i += stride)
            {
                set.Add(i);
            }
            return true;
        }

        private bool ParseWildcard(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsWildcard())
            {
                return false;
            }

            NextSymbol();
            if (Accept('/'))
            {
                NextSymbol();
                var stride = (int)readFunc();

                for (var i = limit.Min; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
            }
            else
            {
                set.AddRange(Range(limit.Min, limit.Max));
            }
            return true;
        }

        private bool ParseSimpleWildcard(CronLambda set, Limit limit)
        {
            if (!IsWildcard())
            {
                return false;
            }

            NextSymbol();
            set.AddRange(Range(limit.Min, limit.Max));
            return true;
        }

        private void ParseBlocks(params Func<bool>[] blocks)
        {
            while (true)
            {
                var found = false;

                foreach (var block in blocks)
                {
                    if (block())
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new Exception($"Unexpected symbol: {Symbol}");
                }

                if (IsSeparator())
                {
                    NextSymbol();
                    continue;
                }

                if (IsEof() || IsWhiteSpace())
                {
                    break;
                }
                throw new Exception($"Unexpected symbol: {Symbol}");
            }
        }

        private void SetContext(string header)
        {
            _context = header;
            var content = new StringBuilder();

            var pos = _position;
            while (Symbol != Eof && !IsWhiteSpace())
            {
                content.Append(Symbol);
                NextSymbol();
            }

            _position = pos;
            Console.WriteLine($"Header: {header}, content: [{content.ToString()}]");
        }
    }
}
