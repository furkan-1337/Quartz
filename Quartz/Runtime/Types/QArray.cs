using System;
using System.Collections.Generic;
using Quartz.Interfaces;

namespace Quartz.Runtime.Types
{
    internal class QArray : QInstance
    {
        public List<object> Elements { get; }

        public QArray(List<object> elements) : base(null) 
        {
            Elements = elements;
        }

        public override object Get(Parsing.Token name)
        {
            
            if (name.Value == "len") return new NativeFunction(Len);
            if (name.Value == "push") return new NativeFunction(Push);
            if (name.Value == "pop") return new NativeFunction(Pop);

            throw new Exceptions.RuntimeError(name, $"Undefined property '{name.Value}'.");
        }

        public override void Set(Parsing.Token name, object value)
        {
            throw new Exceptions.RuntimeError(name, "Cannot add properties to arrays.");
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", Elements) + "]";
        }

        private object Len(Interpreter interpreter, List<object> arguments)
        {
            return Elements.Count;
        }

        private object Push(Interpreter interpreter, List<object> arguments)
        {
            foreach (var arg in arguments)
            {
                Elements.Add(arg);
            }
            return null;
        }

        private object Pop(Interpreter interpreter, List<object> arguments)
        {
            if (Elements.Count == 0) return null;
            object last = Elements[Elements.Count - 1];
            Elements.RemoveAt(Elements.Count - 1);
            return last;
        }
    }

    
    internal class NativeFunction : ICallable
    {
        private Func<Interpreter, List<object>, object> function;

        public NativeFunction(Func<Interpreter, List<object>, object> function)
        {
            this.function = function;
        }

        public int Arity() => -1; 

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return function(interpreter, arguments);
        }
    }
}

