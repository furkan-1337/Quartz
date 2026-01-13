using System;
using System.Collections.Generic;
using System.Linq;
using Quartz.Parsing;

namespace Quartz.Runtime.Types
{
    internal class QDictionary : QInstance
    {
        public Dictionary<object, object> Elements { get; }

        public QDictionary(Dictionary<object, object> elements) : base(null) 
        {
            Elements = elements;

            
            
            
            
            
        }

        public override object Get(Token name)
        {
            
            switch (name.Value)
            {
                case "len": return new NativeMethod(this, (args) => (double)Elements.Count, 0);
                case "keys": return new NativeMethod(this, (args) => new QArray(Elements.Keys.ToList()), 0);
                case "values": return new NativeMethod(this, (args) => new QArray(Elements.Values.ToList()), 0);
                case "remove": return new NativeMethod(this, (args) => Elements.Remove(args[0]), 1);
                case "clear": return new NativeMethod(this, (args) => { Elements.Clear(); return null; }, 0);
                case "contains": return new NativeMethod(this, (args) => Elements.ContainsKey(args[0]), 1);
            }

            return base.Get(name);
        }

        public override string ToString()
        {
            var pairs = Elements.Select(kv => $"{Stringify(kv.Key)}: {Stringify(kv.Value)}");
            return "{" + string.Join(", ", pairs) + "}";
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "null";
            if (obj is string s) return $"\"{s}\"";
            return obj.ToString();
        }

        
        private class NativeMethod : Quartz.Interfaces.ICallable
        {
            private readonly QDictionary _instance;
            private readonly Func<List<object>, object> _func;
            private readonly int _arity;

            public NativeMethod(QDictionary instance, Func<List<object>, object> func, int arity)
            {
                _instance = instance;
                _func = func;
                _arity = arity;
            }

            public int Arity() => _arity;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return _func(arguments);
            }

            public override string ToString() => "<native fn>";
        }
    }
}

