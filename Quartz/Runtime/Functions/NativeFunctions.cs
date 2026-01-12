using System;
using System.Collections.Generic;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Functions
{
    internal class TypeOfFunction : ICallable
    {
        public int Arity() => 1;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            object value = arguments[0];
            if (value == null) return "null";
            if (value is int) return "int";
            if (value is double) return "double";
            if (value is bool) return "bool";
            if (value is string) return "string";
            if (value is List<object>) return "array";
            if (value is ICallable) return "function";

            return "unknown";
        }

        public override string ToString() => "<native fn typeof>";
    }
}

