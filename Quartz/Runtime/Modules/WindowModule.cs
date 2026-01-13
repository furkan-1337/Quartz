using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Modules
{
    internal class WindowModule : Module
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowTextA(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        static extern int GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLongA(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_ALPHA = 0x2;

        public WindowModule() : base(new Environment())
        {
            ExportedEnv.Define("find", new NativeFind());
            ExportedEnv.Define("getForeground", new NativeGetForeground());
            ExportedEnv.Define("setTitle", new NativeSetTitle());
            ExportedEnv.Define("show", new NativeShow());
            ExportedEnv.Define("exists", new NativeExists());
            ExportedEnv.Define("setOpacity", new NativeSetOpacity());
            ExportedEnv.Define("getRect", new NativeGetRect());
            ExportedEnv.Define("setTopmost", new NativeSetTopmost());
        }

        private class NativeFind : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string className = arguments[0] as string;
                string title = arguments[1] as string;
                IntPtr hwnd = FindWindowA(className!, title!);
                return new QPointer((long)hwnd);
            }
            public override string ToString() => "<native fn Window.find>";
        }

        private class NativeGetForeground : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return new QPointer((long)GetForegroundWindow());
            }
            public override string ToString() => "<native fn Window.getForeground>";
        }

        private class NativeSetTitle : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    string title = arguments[1]?.ToString() ?? string.Empty;
                    return SetWindowTextA((IntPtr)ptr.Address, title);
                }
                return false;
            }
            public override string ToString() => "<native fn Window.setTitle>";
        }

        private class NativeShow : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    int cmd = Convert.ToInt32(arguments[1]);
                    return ShowWindow((IntPtr)ptr.Address, cmd);
                }
                return false;
            }
            public override string ToString() => "<native fn Window.show>";
        }

        private class NativeExists : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    return IsWindow((IntPtr)ptr.Address);
                }
                return false;
            }
            public override string ToString() => "<native fn Window.exists>";
        }

        private class NativeSetOpacity : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    IntPtr hwnd = (IntPtr)ptr.Address;
                    byte alpha = (byte)Convert.ToInt32(arguments[1]);

                    int style = GetWindowLongA(hwnd, GWL_EXSTYLE);
                    SetWindowLongA(hwnd, GWL_EXSTYLE, style | WS_EX_LAYERED);
                    return SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
                }
                return false;
            }
            public override string ToString() => "<native fn Window.setOpacity>";
        }

        private class NativeGetRect : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    if (GetWindowRect((IntPtr)ptr.Address, out RECT rect))
                    {
                        var dict = new Dictionary<string, object>();
                        dict["left"] = rect.Left;
                        dict["top"] = rect.Top;
                        dict["right"] = rect.Right;
                        dict["bottom"] = rect.Bottom;
                        dict["width"] = rect.Right - rect.Left;
                        dict["height"] = rect.Bottom - rect.Top;
                        return dict;
                    }
                }
                return null;
            }
            public override string ToString() => "<native fn Window.getRect>";
        }

        private class NativeSetTopmost : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    bool topmost = (bool)arguments[1];
                    IntPtr HWND_TOPMOST = new IntPtr(-1);
                    IntPtr HWND_NOTOPMOST = new IntPtr(-2);
                    const uint SWP_NOSIZE = 0x0001;
                    const uint SWP_NOMOVE = 0x0002;

                    return SetWindowPos((IntPtr)ptr.Address, topmost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
                return false;
            }
            public override string ToString() => "<native fn Window.setTopmost>";
        }
    }
}

