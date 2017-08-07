using System;
using System.Collections.Generic;
using System.Text;

namespace Cron
{
    public abstract class RecursiveDecentParserBase
    {
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
            return Symbol == '?' || Symbol == '*';
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
