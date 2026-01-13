using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Native;

namespace Quartz.Runtime.Modules
{
    internal class MathModule : Module
    {
        public MathModule() : base(new Environment())
        {
            
            ExportedEnv.Define("PI", Math.PI);
            ExportedEnv.Define("E", Math.E);

            
            ExportedEnv.Define("abs", new NativeMathOneArg(Math.Abs));
            ExportedEnv.Define("ceil", new NativeMathOneArg(Math.Ceiling));
            ExportedEnv.Define("floor", new NativeMathOneArg(Math.Floor));
            ExportedEnv.Define("sqrt", new NativeMathOneArg(Math.Sqrt));
            ExportedEnv.Define("sin", new NativeMathOneArg(Math.Sin));
            ExportedEnv.Define("cos", new NativeMathOneArg(Math.Cos));
            ExportedEnv.Define("tan", new NativeMathOneArg(Math.Tan));

            ExportedEnv.Define("pow", new NativeMathTwoArgs(Math.Pow));
            ExportedEnv.Define("round", new NativeMathOneArg(Math.Round));
            ExportedEnv.Define("min", new NativeMathTwoArgs(Math.Min));
            ExportedEnv.Define("max", new NativeMathTwoArgs(Math.Max));
        }

        
        private class NativeMathOneArg : ICallable
        {
            private Func<double, double> func;

            public NativeMathOneArg(Func<double, double> func)
            {
                this.func = func;
            }

            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                
                double arg = Convert.ToDouble(arguments[0]);
                return func(arg);
            }

            public override string ToString() => "<native fn>";
        }

        private class NativeMathTwoArgs : ICallable
        {
            private Func<double, double, double> func;

            public NativeMathTwoArgs(Func<double, double, double> func)
            {
                this.func = func;
            }

            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                double arg1 = Convert.ToDouble(arguments[0]);
                double arg2 = Convert.ToDouble(arguments[1]);
                return func(arg1, arg2);
            }

            public override string ToString() => "<native fn>";
        }


    }
}


