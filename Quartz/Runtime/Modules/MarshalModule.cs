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
            ExportedEnv.Define("writeByte", new NativeWriteByte());
            ExportedEnv.Define("readInt16", new NativeReadInt16());
            ExportedEnv.Define("writeInt16", new NativeWriteInt16());
            ExportedEnv.Define("readInt64", new NativeReadInt64());
            ExportedEnv.Define("writeInt64", new NativeWriteInt64());
            ExportedEnv.Define("readDouble", new NativeReadDouble());
            ExportedEnv.Define("writeDouble", new NativeWriteDouble());
            ExportedEnv.Define("writeStruct", new NativeWriteStruct());
            ExportedEnv.Define("readStruct", new NativeReadStruct());
            ExportedEnv.Define("structureToPtr", new NativeStructureToPtr());
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
            public int Arity() => 1; 
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
                    return (int)Marshal.ReadByte((IntPtr)ptr.Address); 
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readByte>";
        }

        private class NativeWriteByte : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    int val = Convert.ToInt32(arguments[1]);
                    Marshal.WriteByte((IntPtr)ptr.Address, (byte)val);
                    return null;
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.writeByte>";
        }

        private class NativeReadInt16 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr) return (int)Marshal.ReadInt16((IntPtr)ptr.Address);
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readInt16>";
        }

        private class NativeWriteInt16 : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    short val = Convert.ToInt16(arguments[1]);
                    Marshal.WriteInt16((IntPtr)ptr.Address, val);
                    return null;
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.writeInt16>";
        }

        private class NativeReadInt64 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                
                
                
                if (arguments[0] is QPointer ptr) return Marshal.ReadInt64((IntPtr)ptr.Address);
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readInt64>";
        }

        private class NativeWriteInt64 : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    long val = Convert.ToInt64(arguments[1]);
                    Marshal.WriteInt64((IntPtr)ptr.Address, val);
                    return null;
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.writeInt64>";
        }

        private class NativeReadDouble : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    long val = Marshal.ReadInt64((IntPtr)ptr.Address);
                    return BitConverter.Int64BitsToDouble(val);
                }
                throw new Exception("Expected Pointer.");
            }
            public override string ToString() => "<native fn Marshal.readDouble>";
        }

        private class NativeWriteDouble : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is QPointer ptr)
                {
                    double val = Convert.ToDouble(arguments[1]);
                    Marshal.WriteInt64((IntPtr)ptr.Address, BitConverter.DoubleToInt64Bits(val));
                    return null;
                }
                throw new Exception("Expected Pointer.");
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
                if (arguments[0] is QPointer ptr && arguments[1] is QStructInstance instance)
                {
                    IntPtr currentAddr = (IntPtr)ptr.Address;
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
                                Marshal.WriteInt64(currentAddr, Convert.ToInt64(val));
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
                if (arguments[0] is QPointer ptr && arguments[1] is QStruct template)
                {
                    QStructInstance instance = new QStructInstance(template);
                    IntPtr currentAddr = (IntPtr)ptr.Address;

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
    }
}


