using System;
using System.Collections.Generic;
using System.Threading;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Modules
{
    internal class ThreadModule : Module
    {
        public ThreadModule() : base(new Environment())
        {
            ExportedEnv.Define("sleep", new SleepFunction());
        }

        private class SleepFunction : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int ms = Convert.ToInt32(arguments[0]);
                Thread.Sleep(ms);
                return null;
            }

            public override string ToString() => "<native fn Thread.sleep>";
        }
    }
}

