using System;
using System.Collections.Generic;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Native
{
    internal class TimeFunction : ICallable
    {
        public int Arity() => 0; 

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            
            return (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public override string ToString() => "<native fn time>";
    }
}


