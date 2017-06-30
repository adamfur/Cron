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
        private decimal? _decimal;
        private string _context;

        public CronParser(IMonthLookupFactory factory)
        {
            _factory = factory;
        }

        public override ICronScheduler Parse(string cron)
        {
            Prepare(cron);
            Parse();
            return new CronScheduler(dt => _year.Allowed(dt), dt => _seconds.Allowed(dt), dt => _hours.Allowed(dt), dt => _minutes.Allowed(dt), dt => _months.Allowed(dt), dt => _dayOfMonth.Allowed(dt), dt => _dayOfWeek.Allowed(dt), _decimal ?? 0);
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

        private void ParseYear()
        {
            SkipSpaces();
            SetContext("ParseYear");

            if (IsWildcard())
            {
                NextSymbol();
            }
            else if (IsDigit() || Accept('*') || Accept('/'))
            {
                _year.AddRange(ReadRange(2099));
                while (Accept(','))
                {
                    NextSymbol();
                    _year.AddRange(ReadRange(2099));
                }
            }

            Expect(IsEof(), "More data...");
        }

        private IEnumerable<decimal> ReadRange(decimal limit = 59m)
        {
            return ReadRange(() => ReadDecimal(), limit);
        }

        private IEnumerable<decimal> ReadRange(Func<decimal> action, decimal limit = 59.0m)
        {
            var result = new HashSet<decimal>();
            var start = action();

            Expect(!HasFractions(start), $"Start: {start}");
            if (Accept('/'))
            {
                NextSymbol();
                var end = action();

                result.Add(start);
                if (end == 0m)
                {
                    throw new Exception("Wont divide by zero");
                }

                // no fractions
                if (end % 1m == 0m)
                {
                    for (var i = (int)start; i <= limit; i += (int)end)
                    {
                        result.Add(i);
                    }
                }
                else
                {
                    _decimal = end;
                }
            }
            else if (Accept('-'))
            {
                NextSymbol();
                var end = action();

                for (var i = (int)start; i <= end; ++i)
                {
                    result.Add(i);
                }
            }
            else
            {
                result.Add(start);
            }

            return result;
        }

        private bool HasFractions(decimal value)
        {
            return value % 1m != 0m;
        }

        private string ReadWord()
        {
            var builder = new StringBuilder();

            while (IsAlpha())
            {
                builder.Append(Symbol);
                NextSymbol();
            }

            return builder.ToString();
        }

        private decimal ReadDecimal()
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

            if (Accept('.'))
            {
                builder.Append('.');
                NextSymbol();

                while (IsDigit())
                {
                    builder.Append(Symbol);
                    NextSymbol();
                }
            }

            return Decimal.Parse(builder.ToString());
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

        private bool Accept(char token)
        {
            return token == Symbol;
        }

        private void Expect(char token)
        {
            if (Symbol != token)
            {
                throw new ArgumentException($"Expected token: {token}, got: {Symbol}");
            }
        }

        private void Expect(bool condition, string reason)
        {
            if (!condition)
            {
                throw new ArgumentException($"Expected: {reason}");
            }
        }

        private void ParseDayOfWeek()
        {
            SkipSpaces();
            SetContext("ParseDayOfWeek");

            var limit = new Limit(0, 9);

            ParseBlocks(() => ParseWildcard2(_dayOfWeek, limit), () => ParseDayOfWeekInteger(_dayOfWeek, limit, ReadDayOfWeek), () => ParseDayOfWeek(_dayOfWeek, limit, ReadDayOfWeek));
        }

        private bool ParseDayOfWeekInteger(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsDigit())
            {
                return false;
            }

            var start = (int)readFunc();

            if (Accept('/'))
            {
                NextSymbol();
                var stride = (int)readFunc();

                for (var i = start; i <= limit.Max; i += stride)
                {
                    set.Add(i);
                }
                //set.Add(start);
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
                var end = (int)readFunc();

                for (var i = start; i <= end; ++i)
                {
                    set.Add(i);
                }
                //set.Add(start);
            }
            else if (Accept('L'))
            {
                NextSymbol();
                set.AddLambda(dt =>
                {
                    var dow = (DayOfWeek) start;
                    return _factory.Create(dt, new int[0]).LastWeekday(dow, dt);
                });
            }
            else
            {
                set.Add(start);
            }
            return true;
        }

        private bool ParseDayOfWeek(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsAlpha())
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
                return ReadDecimal();
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
                return (int)_dayTextTranslator[ReadWord()];
            }
            else if (IsDigit())
            {
                return (int)_dayNumberTranslator[(int)ReadDecimal()];
            }
            throw new CronException($"ReadDayOfWeek: [{Symbol}]");
        }

        private void ParseMonth()
        {
            SkipSpaces();
            SetContext("ParseMonth");

            if (IsWildcard())
            {
                // Console.WriteLine($"Seconds: All");
                NextSymbol();
            }
            else
            {
                _months.AddRange(ReadRange(ReadMonth));
                while (Accept(','))
                {
                    NextSymbol();
                    _months.AddRange(ReadRange(ReadMonth));
                }
            }
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
                minusOffset = (int)ReadDecimal();
            }

            span.AddLambda((dt) => (dt == dt.LastOfMonth().AddDays(-minusOffset)));
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

        private void ParseDayOfMonth()
        {
            var limit = new Limit(1, 31);

            SkipSpaces();
            SetContext("ParseDayOfMonth");
            ParseBlocks(() => ParseWildcard(_dayOfMonth, limit, ReadMonth), () => ParseDigitWithWeek(_dayOfMonth, limit, ReadMonth), () => ParseSlash(_dayOfMonth, limit, ReadMonth), () => ParseWeek(_dayOfMonth, limit, _dayOfMonth), () => ParseLast(_dayOfMonth, limit, ReadMonth, _dayOfMonth));
        }

        private void ParseHours()
        {
            var limit = new Limit(0, 23);

            SkipSpaces();
            SetContext("ParseMinutes");
            ParseBlocks(() => ParseWildcard(_hours, limit, ReadInteger), () => ParseDigit(_hours, limit, ReadInteger), () => ParseSlash(_hours, limit, ReadInteger));
        }

        private void ParseMinutes()
        {
            var limit = new Limit(0, 59);

            SkipSpaces();
            SetContext("ParseMinutes");
            ParseBlocks(() => ParseWildcard(_minutes, limit, ReadInteger), () => ParseDigit(_minutes, limit, ReadInteger), () => ParseSlash(_minutes, limit, ReadInteger));
        }

        private void ParseSeconds()
        {
            var limit = new Limit(0, 59);

            SkipSpaces();
            SetContext("ParseSeconds");
            ParseBlocks(() => ParseWildcard(_seconds, limit, ReadInteger), () => ParseDigit2(_seconds, limit, ReadInteger), () => ParseSlash(_seconds, limit, ReadInteger));
        }

        private bool ParseDigit2(CronLambda set, Limit limit, Func<decimal> readFunc)
        {
            if (!IsDigit())
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
            //Console.WriteLine($"[***] ParseDigit {this._context}: {start}, Symbol: {Symbol}");
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
            if (!Slash())
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
            if (!IsWildcard2())
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

        private bool ParseWildcard2(CronLambda set, Limit limit)
        {
            if (!IsWildcard2())
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

                if (IsComma())
                {
                    NextSymbol();
                    continue;
                }

                break;
            }
        }

        private void SkipSpaces()
        {
            while (IsWhiteSpace())
            {
                NextSymbol();
            }
        }

        private void NextSymbol()
        {
            ++_position;
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

        private bool IsWhiteSpace()
        {
            return IsWhiteSpace(Symbol);
        }

        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t';
        }

        private bool Content()
        {
            return IsWhiteSpace(Symbol) || Symbol == Eof;
        }

        private bool IsDigit()
        {
            return '0' <= Symbol && Symbol <= '9';
        }

        private bool IsAlpha()
        {
            return 'A' <= Symbol && Symbol <= 'Z';
        }

        private bool IsWildcard()
        {
            return (Symbol == '?' || Symbol == '*') && (IsWhiteSpace(Peek) || Peek == Eof);
        }

        private bool IsWildcard2()
        {
            return (Symbol == '?' || Symbol == '*');
        }

        private bool IsEof()
        {
            return Symbol == Eof;
        }

        private bool Slash()
        {
            return Symbol == '/';
        }

        private bool IsComma()
        {
            return Symbol == ',';
        }

        private void Prepare(string cron)
        {
            var bytes = Encoding.UTF8.GetBytes(cron.ToUpper());
            _symbols = Encoding.UTF8.GetChars(bytes);
            NextSymbol();
        }

        private IEnumerable<decimal> Range(int min, int max)
        {
            for (var i = min; i <= max; ++i)
            {
                yield return (decimal)i;
            }
        }
    }
}
