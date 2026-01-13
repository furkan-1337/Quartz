using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Runtime
{
    public class Environment
    {
        private Dictionary<string, object?>? values;
        public IReadOnlyDictionary<string, object?> Values => values ?? (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>();

        private List<object?>? slots;

        private static readonly Stack<Environment> _pool = new Stack<Environment>();
        private static readonly object _poolLock = new object();

        public static Environment Rent(Environment? enclosing)
        {
            lock (_poolLock)
            {
                if (_pool.Count > 0)
                {
                    var env = _pool.Pop();
                    env.Initialize(enclosing);
                    return env;
                }
            }
            return new Environment(enclosing);
        }

        public static void Return(Environment env)
        {
            env.Initialize(null);
            lock (_poolLock)
            {
                _pool.Push(env);
            }
        }

        private void Initialize(Environment? enclosing)
        {
            this.enclosing = enclosing;
            if (values != null) values.Clear();
            if (slots != null) slots.Clear();
        }

        private void Reset()
        {
            this.enclosing = null;
            if (values != null) values.Clear();
            if (slots != null) slots.Clear();
        }

        private Environment? enclosing;

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment? enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object? value)
        {
            if (values == null) values = new Dictionary<string, object?>();
            values[name] = value;
        }

        public void DefineSlot(int index, object? value)
        {
            if (slots == null) slots = new List<object?>(index + 1);
            while (slots.Count <= index) slots.Add(null);
            slots[index] = value;
        }

        public object? Get(string name)
        {
            if (values != null && values.TryGetValue(name, out var value))
            {
                return value;
            }

            if (enclosing != null) return enclosing.Get(name);

            throw new Exception($"Undefined variable '{name}'.");
        }

        public object? GetAt(int distance, int index)
        {
            return Ancestor(distance).slots![index];
        }

        public void AssignAt(int distance, int index, object? value)
        {
            Ancestor(distance).slots![index] = value;
        }

        private Environment Ancestor(int distance)
        {
            Environment? env = this;
            for (int i = 0; i < distance; i++)
            {
                env = env?.enclosing;
            }
            return env!;
        }

        public void Assign(string name, object? value)
        {
            if (values != null && values.ContainsKey(name))
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


