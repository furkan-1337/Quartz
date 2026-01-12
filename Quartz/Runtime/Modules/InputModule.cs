using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class InputModule : Module
    {
        public InputModule() : base(new Environment())
        {
            ExportedEnv.Define("isKeyDown", new NativeIsKeyDown());
            ExportedEnv.Define("getMousePos", new NativeGetMousePos());
        }

        private class NativeIsKeyDown : ICallable
        {
            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(int vKey);

            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int vKey = Convert.ToInt32(arguments[0]);
                return (GetAsyncKeyState(vKey) & 0x8000) != 0;
            }
            public override string ToString() => "<native fn Input.isKeyDown>";
        }

        private class NativeGetMousePos : ICallable
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            private static extern bool GetCursorPos(out POINT lpPoint);

            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                var dict = new Dictionary<string, object>();
                if (GetCursorPos(out POINT lpPoint))
                {
                    dict["x"] = lpPoint.X;
                    dict["y"] = lpPoint.Y;
                }
                else
                {
                    dict["x"] = 0;
                    dict["y"] = 0;
                }
                return dict;
            }
            public override string ToString() => "<native fn Input.getMousePos>";
        }
    }
}
