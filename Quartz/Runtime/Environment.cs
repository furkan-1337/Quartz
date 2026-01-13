using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Runtime
{
    public class Environment
    {
        private readonly Dictionary<string, object> values = new();
        public IReadOnlyDictionary<string, object> Values => values;
        private readonly Environment enclosing;

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            values[name] = value;
        }

        public object Get(string name)
        {
            if (values.TryGetValue(name, out var value))
            {
                return value;
            }

            if (enclosing != null) return enclosing.Get(name);

            throw new Exception($"Undefined variable '{name}'.");
        }

        public void Assign(string name, object value)
        {
            if (values.ContainsKey(name))
            {
                values[name] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new Exception($"Undefined variable '{name}'.");
        }
    }
}


