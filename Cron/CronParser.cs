using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cron
{
    public class Limit
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public Limit(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public class Span<T>
    {
        public T Item { get; set; }
    }

    public class CronParser : CronParent
    {
        private Func<DateTime, bool> _year = (value) => true;
        private Func<DateTime, bool> _seconds = (value) => true;
        private Func<DateTime, bool> _hours = (value) => true;
        private Func<DateTime, bool> _minutes = (value) => true;
        private Func<DateTime, bool> _months = (value) => true;
        private Func<DateTime, bool> _dayOfMonth = (value) => true;
        private Func<DateTime, bool> _dayOfWeek = (value) => true;
        private decimal? _decimal;
        private string _context;

        public override ICronScheduler Parse(string cron)
        {
            Prepare(cron);
            Parse();

            return new CronScheduler(_year, _seconds, _hours, _minutes, _months, _dayOfMonth, _dayOfWeek, _decimal ?? 0);
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
                var years = new HashSet<decimal>();

                years.AddRange(ReadRange(2099));
                while (Accept(','))
                {
                    NextSymbol();
                    years.AddRange(ReadRange(2099));
                }

                _year = x => years.Contains(x.Year);
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
                    Console.WriteLine($"Setting decimal?: {end}");
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

            if (IsWildcard())
            {
                // Console.WriteLine($"Seconds: All");
                NextSymbol();
            }
            else
            {
                var dayOfWeek = new HashSet<decimal>();

                dayOfWeek.AddRange(ReadRange(ReadDayOfWeek));
                while (Accept(','))
                {
                    NextSymbol();
                    dayOfWeek.AddRange(ReadRange(ReadDayOfWeek));
                }

                _dayOfWeek = x => dayOfWeek.Contains((int)x.DayOfWeek);
            }
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
            throw new CronException("ReadDayOfWeek");
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
                var months = new HashSet<decimal>();

                months.AddRange(ReadRange(ReadMonth));
                while (Accept(','))
                {
                    NextSymbol();
                    months.AddRange(ReadRange(ReadMonth));
                }

                _months = x => months.Contains(x.Month);
            }
        }

        private bool ParseLast(ISet<decimal> set, Limit limit, Func<decimal> readFunc, Span<Func<DateTime, bool>> span)
        {
            if (!Accept('L'))
            {
                return false;
            }

            var minusOffset = 0;

            NextSymbol();
            if (Accept('-'))
            {
                NextSymbol();
                minusOffset = (int)ReadDecimal();
            }

            var method = span.Item;
            span.Item = (dt) => (dt == dt.LastOfMonth().AddDays(-minusOffset)) || method(dt);
            return true;
        }

        private bool ParseWeek(ISet<decimal> set, Limit limit, Span<Func<DateTime, bool>> span)
        {
            if (!Accept('W'))
            {
                return false;
            }

            Console.WriteLine("parse week");
            NextSymbol();

            var method = span.Item;
            span.Item = (dt) => (dt.DayOfWeek != DayOfWeek.Sunday && dt.DayOfWeek != DayOfWeek.Saturday) || method(dt);
            //span.Item = (dt) => (dt.DayOfWeek != DayOfWeek.Sunday && dt.DayOfWeek != DayOfWeek.Saturday) || span.Item(dt);
            return true;
        }

        private void ParseDayOfMonth()
        {
            var limit = new Limit(1, 31);
            var dayOfMonth = new HashSet<decimal>();
            Func<DateTime, bool> result = (dt) => false;
            var span = new Span<Func<DateTime, bool>> { Item = result };

            SkipSpaces();
            SetContext("ParseDayOfMonth");
            ParseBlocks(() => ParseWildcard(dayOfMonth, limit, ReadMonth), () => ParseDigit(dayOfMonth, limit, ReadMonth), () => ParseSlash(dayOfMonth, limit, ReadMonth), () => ParseWeek(dayOfMonth, limit, span), () => ParseLast(dayOfMonth, limit, ReadMonth, span));

            var inner = span.Item;
            _dayOfMonth = (dt) => dayOfMonth.Contains(dt.Day) || inner(dt);
        }

        private void ParseHours()
        {
            var limit = new Limit(0, 23);
            var hours = new HashSet<decimal>();

            SkipSpaces();
            SetContext("ParseMinutes");
            ParseBlocks(() => ParseWildcard(hours, limit, ReadInteger), () => ParseDigit(hours, limit, ReadInteger), () => ParseSlash(hours, limit, ReadInteger));
            _hours = dt => hours.Contains(dt.Hour);
        }

        private void ParseMinutes()
        {
            var limit = new Limit(0, 59);
            var minutes = new HashSet<decimal>();

            SkipSpaces();
            SetContext("ParseMinutes");
            ParseBlocks(() => ParseWildcard(minutes, limit, ReadInteger), () => ParseDigit(minutes, limit, ReadInteger), () => ParseSlash(minutes, limit, ReadInteger));
            _minutes = dt => minutes.Contains(dt.Minute);
        }

        private void ParseSeconds()
        {
            var limit = new Limit(0, 59);
            var seconds = new HashSet<decimal>();

            SkipSpaces();
            SetContext("ParseSeconds");
            ParseBlocks(() => ParseWildcard(seconds, limit, ReadInteger), () => ParseDigit(seconds, limit, ReadInteger), () => ParseSlash(seconds, limit, ReadInteger));
            _seconds = dt => seconds.Contains(dt.Second);
        }

        private bool ParseDigit(ISet<decimal> set, Limit limit, Func<decimal> readFunc)
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
            else if (IsWhiteSpace())
            {
            }
            return true;
        }

        private bool ParseSlash(ISet<decimal> set, Limit limit, Func<decimal> readFunc)
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

        private bool ParseWildcard(ISet<decimal> set, Limit limit, Func<decimal> readFunc)
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
                set.AddRange(Range(0, 59));
            }
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
