using System;
using System.Collections.Generic;
using System.Linq;

namespace Cron
{
    public abstract class CronLambda
    {
        private HashSet<decimal> _set = new HashSet<decimal>();
        private List<Func<DateTime, bool>> _funcs = new List<Func<DateTime, bool>>();
        protected bool _base = true;

        public bool Allowed(DateTime dt)
        {
            var val = (int) Property(dt);
            var result = _funcs.Any(x => x(dt)) || _set.Contains(val) || _base;
            return result;
        }

        public void Add(int value)
        {
            _set.Add(value);
            _base = false;
        }

        public void AddRange(IEnumerable<decimal> set)
        {
            _set.AddRange(set);
            _base = false;
        }

        public void AddLambda(Func<DateTime, bool> func)
        {
            _funcs.Add(func);
            _base = false;
        }

        protected abstract decimal Property(DateTime dt);
    }
}
