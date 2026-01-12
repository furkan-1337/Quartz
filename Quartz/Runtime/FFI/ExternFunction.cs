using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.FFI
{
    internal class ExternFunction : ICallable
    {
        public int Arity() => -1; // Variadic

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments.Count < 3)
                throw new Exception("Extern requires at least 3 arguments: dll, returnType, functionName.");

            string dllName = (string)arguments[0];
            string returnTypeStr = (string)arguments[1];
            string functionName = (string)arguments[2];

            IntPtr lib = Interop.LoadLibrary(dllName);
            if (lib == IntPtr.Zero)
                throw new Exception($"Could not load library '{dllName}'.");

            IntPtr proc = Interop.GetProcAddress(lib, functionName);
            if (proc == IntPtr.Zero)
                throw new Exception($"Could not find function '{functionName}' in '{dllName}'.");

            Type returnType = ParseType(returnTypeStr);
            List<Type> paramTypes = new List<Type>();

            for (int i = 3; i < arguments.Count; i++)
            {
                paramTypes.Add(ParseType((string)arguments[i]));
            }

            return new NativeCallable(proc, returnType, paramTypes);
        }

        private Type ParseType(string typeName)
        {
            switch (typeName)
            {
                case "int": return typeof(int);
                case "long": return typeof(long);
                case "float": return typeof(float);
                case "double": return typeof(double);
                case "string": return typeof(string);
                case "bool": return typeof(bool);
                case "void": return typeof(void);
                case "pointer": return typeof(IntPtr);
                case "ref int": return typeof(int).MakeByRefType();
                case "out int": return typeof(int).MakeByRefType();
                default: throw new Exception($"Unknown type '{typeName}'.");
            }
        }

        public override string ToString() => "<native fn factory>";
    }
}

