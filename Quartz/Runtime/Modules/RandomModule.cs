using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Native;

namespace Quartz.Runtime.Modules
{
    internal class RandomModule : Module
    {
        private static Random random = new Random();

        public RandomModule() : base(new Environment())
        {
            ExportedEnv.Define("range", new NativeRange());
            ExportedEnv.Define("next", new NativeNext());
        }

        private class NativeNext : ICallable
        {
            public int Arity() => 0;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return random.NextDouble();
            }
            public override string ToString() => "<native fn next>";
        }

        private class NativeRange : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int min = Convert.ToInt32(arguments[0]);
                int max = Convert.ToInt32(arguments[1]);
                return random.Next(min, max);
            }
            public override string ToString() => "<native fn range>";
        }
    }
}


