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
            ExportedEnv.Define("create", new CreateFunction());
            ExportedEnv.Define("getCurrentId", new GetCurrentIdFunction());
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

        private class CreateFunction : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (!(arguments[0] is ICallable callable))
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Thread.create expects a function.");

                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        var newInterp = new Interpreter(interpreter.global);
                        newInterp.DebugMode = interpreter.DebugMode;
                        callable.Call(newInterp, new List<object>());
                    }
                    catch (Exception ex)
                    {
                        if (ex is Exceptions.RuntimeError re)
                        {
                            Exceptions.ErrorReporter.Error(re.Token, re.Message);
                        }
                        else
                        {
                            System.Console.WriteLine($"[Thread Error] {ex.Message}");
                        }
                    }
                });
                thread.Start();
                return null;
            }
            public override string ToString() => "<native fn Thread.create>";
        }

        private class GetCurrentIdFunction : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
            public override string ToString() => "<native fn Thread.getCurrentId>";
        }
    }
}


