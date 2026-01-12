using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Native;

namespace Quartz.Runtime.Modules
{
    internal class ArrayModule : Module
    {
        public ArrayModule() : base(new Environment())
        {
            ExportedEnv.Define("length", new NativeArrayLength());
            ExportedEnv.Define("push", new NativeArrayPush());
            ExportedEnv.Define("pop", new NativeArrayPop());
            ExportedEnv.Define("insert", new NativeArrayInsert());
            ExportedEnv.Define("remove", new NativeArrayRemove());
        }

        private class NativeArrayLength : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is List<object> list)
                    return list.Count;
                if (arguments[0] is string str)
                    return str.Length;
                throw new Exception("Expected array or string.");
            }
            public override string ToString() => "<native fn length>";
        }

        private class NativeArrayPush : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is List<object> list)
                {
                    list.Add(arguments[1]);
                    return list.Count;
                }
                throw new Exception("Expected array as first argument.");
            }
            public override string ToString() => "<native fn push>";
        }

        private class NativeArrayPop : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is List<object> list)
                {
                    if (list.Count == 0) return null;
                    object last = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    return last;
                }
                throw new Exception("Expected array as first argument.");
            }
            public override string ToString() => "<native fn pop>";
        }

        private class NativeArrayInsert : ICallable
        {
            public int Arity() => 3;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is List<object> list)
                {
                    int index = Convert.ToInt32(arguments[1]);
                    object value = arguments[2];
                    if (index >= 0 && index <= list.Count)
                    {
                        list.Insert(index, value);
                        return list.Count;
                    }
                    throw new Exception("Index out of bounds.");
                }
                throw new Exception("Expected array as first argument.");
            }
            public override string ToString() => "<native fn insert>";
        }

        private class NativeArrayRemove : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is List<object> list)
                {
                    int index = Convert.ToInt32(arguments[1]);
                    if (index >= 0 && index < list.Count)
                    {
                        object removed = list[index];
                        list.RemoveAt(index);
                        return removed;
                    }
                    throw new Exception("Index out of bounds.");
                }
                throw new Exception("Expected array as first argument.");
            }
            public override string ToString() => "<native fn remove>";
        }
    }
}

