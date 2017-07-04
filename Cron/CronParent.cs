using System;
using System.Collections.Generic;
using System.Text;

namespace Cron
{
    public abstract class CronParent : ICronParser
    {
        protected static Dictionary<string, int> _monthTranslator = new Dictionary<string, int>
        {
            ["JAN"] = 1,
            ["FEB"] = 2,
            ["MAR"] = 3,
            ["APR"] = 4,
            ["MAY"] = 5,
            ["JUN"] = 6,
            ["JUL"] = 7,
            ["AUG"] = 8,
            ["SEP"] = 9,
            ["OCT"] = 10,
            ["NOV"] = 11,
            ["DEC"] = 12
        };
        protected static Dictionary<string, DayOfWeek> _dayTextTranslator = new Dictionary<string, DayOfWeek>
        {
            ["SUN"] = DayOfWeek.Sunday,
            ["MON"] = DayOfWeek.Monday,
            ["TUE"] = DayOfWeek.Tuesday,
            ["WED"] = DayOfWeek.Wednesday,
            ["THU"] = DayOfWeek.Thursday,
            ["FRI"] = DayOfWeek.Friday,
            ["SAT"] = DayOfWeek.Saturday,
            ["L"] = DayOfWeek.Saturday
        };
        protected static Dictionary<int, DayOfWeek> _dayNumberTranslator = new Dictionary<int, DayOfWeek>
        {
            [1] = DayOfWeek.Sunday,
            [2] = DayOfWeek.Monday,
            [3] = DayOfWeek.Tuesday,
            [4] = DayOfWeek.Wednesday,
            [5] = DayOfWeek.Thursday,
            [6] = DayOfWeek.Friday,
            [7] = DayOfWeek.Saturday,
        };
	
        public abstract ICronScheduler Parse(string cron);
        protected char Eof = '\0';
        protected char[] _symbols;
        protected int _position = -1; 

        protected char Symbol => _position >= _symbols.Length ? Eof : _symbols[_position];

        protected void SkipSpaces()
        {
            while (IsWhiteSpace())
            {
                NextSymbol();
            }
        }

        protected void NextSymbol()
        {
            ++_position;
        }

        protected bool IsWhiteSpace()
        {
            return Symbol == ' ' || Symbol == '\t';
        }
        protected bool Content()
        {
            return IsWhiteSpace() || Symbol == Eof;
        }

        protected bool IsDigit()
        {
            return '0' <= Symbol && Symbol <= '9';
        }

        protected bool IsAlpha()
        {
            return 'A' <= Symbol && Symbol <= 'Z';
        }

        protected bool IsWildcard()
        {
            return (Symbol == '?' || Symbol == '*');
        }

        protected bool IsEof()
        {
            return Symbol == Eof;
        }

        protected bool IsVoid()
        {
            return IsEof() || IsWhiteSpace();
        }        

        protected bool IsSlash()
        {
            return Symbol == '/';
        }

        protected bool IsSeparator()
        {
            return Symbol == ',';
        }

        protected void Prepare(string cron)
        {
            var bytes = Encoding.UTF8.GetBytes(cron.ToUpper());
            _symbols = Encoding.UTF8.GetChars(bytes);
            NextSymbol();
        }

        protected IEnumerable<decimal> Range(int min, int max)
        {
            for (var i = min; i <= max; ++i)
            {
                yield return (decimal)i;
            }
        }

        protected bool Accept(char token)
        {
            return token == Symbol;
        }

        protected void Expect(char token)
        {
            if (Symbol != token)
            {
                throw new ArgumentException($"Expected token: {token}, got: {Symbol}");
            }
        }

        protected void Expect(bool condition, string reason)
        {
            if (!condition)
            {
                throw new ArgumentException($"Expected: {reason}");
            }
        }        
    }        
}
