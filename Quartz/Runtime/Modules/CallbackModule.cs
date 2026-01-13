using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Native
{
    internal class CallbackModule : Module
    {
        
        
        public delegate long Callback4(long a, long b, long c, long d);
        public delegate long Callback2(long a, long b);

        
        private static readonly List<Delegate> _activeCallbacks = new();

        public CallbackModule() : base(new Environment())
        {
            ExportedEnv.Define("create", new NativeCreateCallback());
            ExportedEnv.Define("clear", new NativeClear());
        }

        private class NativeCreateCallback : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is ICallable quartzFunc)
                {
                    
                    Callback4 d = (a, b, c, d_val) =>
                    {
                        try
                        {
                            var args = new List<object> { (long)a, (long)b, (long)c, (long)d_val };
                            object? result = quartzFunc.Call(interpreter, args);
                            return Convert.ToInt64(result ?? 1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[QUARTZ CALLBACK ERROR] {ex.Message}");
                            return 0;
                        }
                    };

                    _activeCallbacks.Add(d);
                    IntPtr ptr = Marshal.GetFunctionPointerForDelegate(d);
                    return new QPointer((long)ptr);
                }
                throw new Exception("Expected function as argument.");
            }
            public override string ToString() => "<native fn Callback.create>";
        }

        private class NativeClear : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                _activeCallbacks.Clear();
                return null;
            }
            public override string ToString() => "<native fn Callback.clear>";
        }
    }
}

