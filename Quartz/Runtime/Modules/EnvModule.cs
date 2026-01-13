using System;
using System.Collections.Generic;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class EnvModule : Module
    {
        public EnvModule() : base(new Environment())
        {
            ExportedEnv.Define("get", new NativeGet());
            ExportedEnv.Define("set", new NativeSet());
        }

        private class NativeGet : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string name = (string)arguments[0];
                return System.Environment.GetEnvironmentVariable(name) ?? "";
            }
            public override string ToString() => "<native fn Env.get>";
        }

        private class NativeSet : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string name = (string)arguments[0];
                string value = (string)arguments[1];
                System.Environment.SetEnvironmentVariable(name, value);
                return null;
            }
            public override string ToString() => "<native fn Env.set>";
        }
    }
}

