using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Native;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Modules
{
    internal class MarshalModule : Module
    {
        public MarshalModule() : base(new Environment())
        {
            ExportedEnv.Define("alloc", new NativeAlloc());
            ExportedEnv.Define("free", new NativeFree());
            ExportedEnv.Define("readInt", new NativeReadInt());
            ExportedEnv.Define("writeInt", new NativeWriteInt());
            ExportedEnv.Define("readString", new NativeReadString());
            ExportedEnv.Define("readByte", new NativeReadByte());
        }

        private class NativeAlloc : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int size = Convert.ToInt32(arguments[0]);
                long addr = (long)Marshal.AllocHGlobal(size);
                return new QPointer(addr);
            }
            public override string ToString() => "<native fn Marshal.alloc>";
        }

        private class NativeFree : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    Marshal.FreeHGlobal((IntPtr)ptr.Address);
                }
                else
                {
                    throw new Exception("Expected Pointer.");
                }
                return null;
            }
            public override string ToString() => "<native fn Marshal.free>";
        }

        private class NativeReadInt : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    return Marshal.ReadInt32((IntPtr)ptr.Address);
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readInt>";
        }

        private class NativeWriteInt : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    int val = Convert.ToInt32(arguments[1]);
                    Marshal.WriteInt32((IntPtr)ptr.Address, val);
                    return null;
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.writeInt>";
        }

        private class NativeReadString : ICallable
        {
            public int Arity() => 1; // Retrieves string from pointer
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    return Marshal.PtrToStringAnsi((IntPtr)ptr.Address);
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readString>";
        }

        private class NativeReadByte : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    return (int)Marshal.ReadByte((IntPtr)ptr.Address); // Return as int (0-255)
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readByte>";
        }
    }
}

