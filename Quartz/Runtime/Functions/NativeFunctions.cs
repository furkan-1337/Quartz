using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Functions
{
    internal class TypeOfFunction : ICallable
    {
        public int Arity() => 1;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            object value = arguments[0];
            if (value == null) return "null";
            if (value is int) return "int";
            if (value is byte) return "byte";
            if (value is double) return "double";
            if (value is bool) return "bool";
            if (value is string) return "string";
            if (value is List<object>) return "array";
            if (value is QInstance instance) return instance.Class.Name;
            if (value is QStructInstance) return "struct";
            if (value is ICallable) return "function";

            return "unknown";
        }

        public override string ToString() => "<native fn typeof>";
    }

    internal class SizeOfFunction : ICallable
    {
        public int Arity() => 1;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            object value = arguments[0];
            if (value == null) return 0;


            if (value is string typeName)
            {
                int size = GetTypeSize(typeName);
                if (size > 0) return size;
                return typeName.Length;
            }


            if (value is int) return 4;
            if (value is byte) return 1;
            if (value is double) return 8;
            if (value is bool) return 1;
            if (value is string s) return s.Length;
            if (value is QPointer) return 8;
            if (value is List<object> list) return list.Count;
            if (value is QStructInstance structInstance)
            {
                return GetStructSize(structInstance.Template);
            }
            if (value is QStruct @struct)
            {
                return GetStructSize(@struct);
            }

            return 0;
        }

        private int GetStructSize(QStruct @struct)
        {
            int totalSize = 0;
            foreach (var field in @struct.Fields)
            {
                totalSize += GetTypeSize(field.Type.Value);
            }
            return totalSize;
        }

        private int GetTypeSize(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "int": return 4;
                case "double": return 8;
                case "bool": return 1;
                case "pointer": return 8;
                case "string": return 8;
                case "float": return 4;
                case "long": return 8;
                case "short": return 2;
                case "byte": return 1;
                default: return 0;
            }
        }

        public override string ToString() => "<native fn sizeof>";
    }
}
