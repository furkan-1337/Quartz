using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.FFI;
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
            ExportedEnv.Define("writeByte", new NativeWriteByte());
            ExportedEnv.Define("readInt16", new NativeReadInt16());
            ExportedEnv.Define("writeInt16", new NativeWriteInt16());
            ExportedEnv.Define("readInt64", new NativeReadInt64());
            ExportedEnv.Define("writeInt64", new NativeWriteInt64());
            ExportedEnv.Define("readDouble", new NativeReadDouble());
            ExportedEnv.Define("writeDouble", new NativeWriteDouble());
            ExportedEnv.Define("writeFloat", new NativeWriteFloat());
            ExportedEnv.Define("readFloat", new NativeReadFloat());
            ExportedEnv.Define("writeStruct", new NativeWriteStruct());
            ExportedEnv.Define("readStruct", new NativeReadStruct());
            ExportedEnv.Define("readPtr", new NativeReadPtr());
            ExportedEnv.Define("writePtr", new NativeWritePtr());
            ExportedEnv.Define("structureToPtr", new NativeStructureToPtr());
            ExportedEnv.Define("stringToPtr", new NativeStringToPtr());
            ExportedEnv.Define("stringToPtrUni", new NativeStringToPtrUni());
            ExportedEnv.Define("callPtr", new NativeCallPtr());
            ExportedEnv.Define("invoke", new NativeInvoke());
            ExportedEnv.Define("size", new NativeSize());
        }

        private static long GetAddress(Interpreter interpreter, object arg, string functionName)
        {
            if (arg is QPointer ptr) return ptr.Address;
            if (arg is IntPtr p) return (long)p;
            try
            {
                return Convert.ToInt64(arg);
            }
            catch
            {
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token { File = "unknown", Line = 0, Column = 0 }, $"[{functionName}] Expected Pointer/Address, got {arg?.GetType().Name ?? "null"}: {arg}");
            }
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
                long addr = GetAddress(interpreter, arguments[0], "Marshal.free");
                Marshal.FreeHGlobal((IntPtr)addr);
                return null;
            }
            public override string ToString() => "<native fn Marshal.free>";
        }

        private class NativeReadInt : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readInt");
                return Marshal.ReadInt32((IntPtr)addr);
            }
            public override string ToString() => "<native fn Marshal.readInt>";
        }

        private class NativeWriteInt : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.writeInt");
                int val;
                if (arguments[1] is IntPtr p) val = (int)p;
                else if (arguments[1] is QPointer qp) val = (int)qp.Address;
                else val = Convert.ToInt32(arguments[1]);

                Marshal.WriteInt32((IntPtr)addr, val);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeInt>";
        }

        private class NativeReadString : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readAnsiString");
                return Marshal.PtrToStringAnsi((IntPtr)addr);
            }
            public override string ToString() => "<native fn Marshal.readString>";
        }

        private class NativeReadByte : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readByte");
                return (int)Marshal.ReadByte((IntPtr)addr);
            }
            public override string ToString() => "<native fn Marshal.readByte>";
        }

        private class NativeWriteByte : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.writeByte");
                int val = Convert.ToInt32(arguments[1]);
                Marshal.WriteByte((IntPtr)addr, (byte)val);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeByte>";
        }

        private class NativeReadInt16 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readInt16");
                return (int)Marshal.ReadInt16((IntPtr)addr);
            }
            public override string ToString() => "<native fn Marshal.readInt16>";
        }

        private class NativeWriteInt16 : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.writeInt16");
                short val = Convert.ToInt16(arguments[1]);
                Marshal.WriteInt16((IntPtr)addr, val);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeInt16>";
        }

        private class NativeReadInt64 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readInt64");
                return Marshal.ReadInt64((IntPtr)addr);
            }
            public override string ToString() => "<native fn Marshal.readInt64>";
        }

        private class NativeWriteInt64 : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.writeInt64");
                long val;
                if (arguments[1] is IntPtr p) val = (long)p;
                else if (arguments[1] is QPointer qp) val = qp.Address;
                else val = Convert.ToInt64(arguments[1]);

                Marshal.WriteInt64((IntPtr)addr, val);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeInt64>";
        }

        private class NativeReadDouble : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.readDouble");
                long val = Marshal.ReadInt64((IntPtr)addr);
                return BitConverter.Int64BitsToDouble(val);
            }
            public override string ToString() => "<native fn Marshal.readDouble>";
        }

        private class NativeWriteDouble : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long addr = GetAddress(interpreter, arguments[0], "Marshal.writeDouble");
                double val = Convert.ToDouble(arguments[1]);
                Marshal.WriteInt64((IntPtr)addr, BitConverter.DoubleToInt64Bits(val));
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeDouble>";
        }

        private class NativeStructureToPtr : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QInstance instance && arguments[1] is QPointer ptr)
                {
                    long currentAddr = ptr.Address;
                    foreach (var kvp in instance.Fields)
                    {
                        object val = kvp.Value;
                        if (val is int i)
                        {
                            Marshal.WriteInt32((IntPtr)currentAddr, i);
                            currentAddr += 4;
                        }
                        else if (val is double d)
                        {
                            Marshal.WriteInt64((IntPtr)currentAddr, BitConverter.DoubleToInt64Bits(d));
                            currentAddr += 8;
                        }
                        else if (val is bool b)
                        {
                            Marshal.WriteInt32((IntPtr)currentAddr, b ? 1 : 0);
                            currentAddr += 4;
                        }
                        else if (val is QPointer p)
                        {
                            if (IntPtr.Size == 8)
                            {
                                Marshal.WriteInt64((IntPtr)currentAddr, p.Address);
                                currentAddr += 8;
                            }
                            else
                            {
                                Marshal.WriteInt32((IntPtr)currentAddr, (int)p.Address);
                                currentAddr += 4;
                            }
                        }
                    }
                    return null;
                }
                throw new Exception("Expected Instance and Pointer.");
            }
            public override string ToString() => "<native fn Marshal.structureToPtr>";
        }

        private class NativeWriteStruct : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long ptrAddr = GetAddress(interpreter, arguments[0], "Marshal.writeStruct");

                if (arguments[1] is QStructInstance instance)
                {
                    IntPtr currentAddr = (IntPtr)ptrAddr;
                    foreach (var field in instance.Template.Fields)
                    {
                        string type = field.Type.Value.ToLower();
                        object val = instance.Get(field.Name);

                        switch (type)
                        {
                            case "int":
                                Marshal.WriteInt32(currentAddr, Convert.ToInt32(val));
                                currentAddr += 4;
                                break;
                            case "double":
                                Marshal.WriteInt64(currentAddr, BitConverter.DoubleToInt64Bits(Convert.ToDouble(val)));
                                currentAddr += 8;
                                break;
                            case "bool":
                                Marshal.WriteByte(currentAddr, (byte)((bool)val ? 1 : 0));
                                currentAddr += 1;
                                break;
                            case "pointer":
                                Marshal.WriteIntPtr(currentAddr, val is QPointer p ? (IntPtr)p.Address : IntPtr.Zero);
                                currentAddr += IntPtr.Size;
                                break;
                            case "long":
                                long lVal;
                                if (val is IntPtr ip) lVal = (long)ip;
                                else if (val is QPointer qp) lVal = qp.Address;
                                else lVal = Convert.ToInt64(val);

                                Marshal.WriteInt64(currentAddr, lVal);
                                currentAddr += 8;
                                break;
                            case "float":
                                byte[] bytes = BitConverter.GetBytes(Convert.ToSingle(val));
                                Marshal.Copy(bytes, 0, currentAddr, 4);
                                currentAddr += 4;
                                break;
                        }
                    }
                    return null;
                }
                throw new Exception("Expected Pointer and StructInstance.");
            }
            public override string ToString() => "<native fn Marshal.writeStruct>";
        }

        private class NativeReadStruct : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long ptrAddr = GetAddress(interpreter, arguments[0], "Marshal.readStruct");

                if (arguments[1] is QStruct template)
                {
                    QStructInstance instance = new QStructInstance(template);
                    IntPtr currentAddr = (IntPtr)ptrAddr;

                    foreach (var field in template.Fields)
                    {
                        string type = field.Type.Value.ToLower();

                        switch (type)
                        {
                            case "int":
                                instance.Set(field.Name, Marshal.ReadInt32(currentAddr));
                                currentAddr += 4;
                                break;
                            case "double":
                                long lVal = Marshal.ReadInt64(currentAddr);
                                instance.Set(field.Name, BitConverter.Int64BitsToDouble(lVal));
                                currentAddr += 8;
                                break;
                            case "bool":
                                instance.Set(field.Name, Marshal.ReadByte(currentAddr) != 0);
                                currentAddr += 1;
                                break;
                            case "pointer":
                                instance.Set(field.Name, new QPointer((long)Marshal.ReadIntPtr(currentAddr)));
                                currentAddr += IntPtr.Size;
                                break;
                            case "long":
                                instance.Set(field.Name, Marshal.ReadInt64(currentAddr));
                                currentAddr += 8;
                                break;
                            case "float":
                                byte[] bytes = new byte[4];
                                Marshal.Copy(currentAddr, bytes, 0, 4);
                                instance.Set(field.Name, BitConverter.ToSingle(bytes, 0));
                                currentAddr += 4;
                                break;
                        }
                    }
                    return instance;
                }
                throw new Exception("Expected Pointer and Struct Definition.");
            }
            public override string ToString() => "<native fn Marshal.readStruct>";
        }

        private class NativeStringToPtr : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string s = Convert.ToString(arguments[0]);
                IntPtr ptr = Marshal.StringToHGlobalAnsi(s);
                return new QPointer((long)ptr);
            }
            public override string ToString() => "<native fn Marshal.stringToPtr>";
        }

        private class NativeStringToPtrUni : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string s = Convert.ToString(arguments[0]);
                IntPtr ptr = Marshal.StringToHGlobalUni(s);
                return new QPointer((long)ptr);
            }
            public override string ToString() => "<native fn Marshal.stringToPtrUni>";
        }

        private class NativeCallPtr : ICallable
        {
            public int Arity() => -1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments.Count < 2)
                    throw new Exception("callPtr requires at least 2 arguments: address, returnType, [paramTypes].");

                long address = GetAddress(interpreter, arguments[0], "Marshal.callPtr");
                string returnTypeStr = (string)arguments[1];

                Type returnType = ExternFunction.ParseType(returnTypeStr);
                List<Type> paramTypes = new List<Type>();

                List<object> typesList = null;
                if (arguments.Count > 2)
                {
                    if (arguments[2] is List<object> l) typesList = l;
                    else if (arguments[2] is QArray arr) typesList = arr.Elements;
                }

                if (typesList != null)
                {
                    foreach (var t in typesList)
                    {
                        paramTypes.Add(ExternFunction.ParseType(Convert.ToString(t)));
                    }
                }

                return new NativeCallable((IntPtr)address, returnType, paramTypes);
            }
            public override string ToString() => "<native fn Marshal.callPtr>";
        }

        private class NativeWriteFloat : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long address = GetAddress(interpreter, arguments[0], "Marshal.writeFloat");

                float val = Convert.ToSingle(arguments[1]);
                byte[] bytes = BitConverter.GetBytes(val);
                Marshal.Copy(bytes, 0, (IntPtr)address, 4);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writeFloat>";
        }

        private class NativeReadFloat : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long address = GetAddress(interpreter, arguments[0], "Marshal.readFloat");

                byte[] bytes = new byte[4];
                Marshal.Copy((IntPtr)address, bytes, 0, 4);
                return (double)BitConverter.ToSingle(bytes, 0);
            }
            public override string ToString() => "<native fn Marshal.readFloat>";
        }

        private class NativeInvoke : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                List<object> argsList = null;
                if (arguments[1] is List<object> l) argsList = l;
                else if (arguments[1] is QArray arr) argsList = arr.Elements;

                if (arguments[0] is ICallable function && argsList != null)
                {
                    return function.Call(interpreter, argsList);
                }
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Expected Function and List/Array of arguments. Got {arguments[0]?.GetType().Name} and {arguments[1]?.GetType().Name}");
            }
            public override string ToString() => "<native fn Marshal.invoke>";
        }
        private class NativeReadPtr : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long address = GetAddress(interpreter, arguments[0], "Marshal.readPtr");
                return new QPointer((long)Marshal.ReadIntPtr((IntPtr)address));
            }
            public override string ToString() => "<native fn Marshal.readPtr>";
        }

        private class NativeWritePtr : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                long address = GetAddress(interpreter, arguments[0], "Marshal.writePtr");
                long val = GetAddress(interpreter, arguments[1], "Marshal.writePtr-value");
                Marshal.WriteIntPtr((IntPtr)address, (IntPtr)val);
                return null;
            }
            public override string ToString() => "<native fn Marshal.writePtr>";
        }

        private class NativeSize : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return IntPtr.Size;
            }
            public override string ToString() => "<native fn Marshal.size>";
        }
    }
}
