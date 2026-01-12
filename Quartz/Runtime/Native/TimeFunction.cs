using System;
using System.Collections.Generic;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Native
{
    internal class TimeFunction : ICallable
    {
        public int Arity() => 0; // No arguments needed

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            // Return current time in milliseconds
            return (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public override string ToString() => "<native fn time>";
    }
}

