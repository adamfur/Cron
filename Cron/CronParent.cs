using System;
using System.Collections.Generic;

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
        protected char Peek => _position + 1 >= _symbols.Length ? Eof : _symbols[_position + 1];               
    }
}
