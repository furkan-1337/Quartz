using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime;
using Quartz.Runtime.Native;
using Quartz.Runtime.Types;

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

            public object Call(Interpreter interpreter, List<object?> arguments)
            {
                if (arguments[0] is QArray qArray)
                    return qArray.Elements.Count;
                if (arguments[0] is List<object> list)
                    return list.Count;
                if (arguments[0] is string str)
                    return str.Length;
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Expected array or string.");
            }
            public override string ToString() => "<native fn length>";
        }

        private class NativeArrayPush : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object?> arguments)
            {
                if (arguments[0] is QArray qArray)
                {
                    qArray.Elements.Add(arguments[1]);
                    return qArray.Elements.Count;
                }
                if (arguments[0] is List<object> list)
                {
                    list.Add(arguments[1]);
                    return list.Count;
                }
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Expected array as first argument.");
            }
            public override string ToString() => "<native fn push>";
        }

        private class NativeArrayPop : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object?> arguments)
            {
                List<object> list = null;
                if (arguments[0] is QArray qArray) list = qArray.Elements;
                else if (arguments[0] is List<object> l) list = l;

                if (list != null)
                {
                    if (list.Count == 0) return null;
                    object last = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    return last;
                }
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Expected array as first argument.");
            }
            public override string ToString() => "<native fn pop>";
        }

        private class NativeArrayInsert : ICallable
        {
            public int Arity() => 3;

            public object Call(Interpreter interpreter, List<object?> arguments)
            {
                List<object> list = null;
                if (arguments[0] is QArray qArray) list = qArray.Elements;
                else if (arguments[0] is List<object> l) list = l;

                if (list != null)
                {
                    int index = Convert.ToInt32(arguments[1]);
                    object value = arguments[2];
                    if (index >= 0 && index <= list.Count)
                    {
                        list.Insert(index, value);
                        return list.Count;
                    }
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Index out of bounds.");
                }
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Expected array as first argument.");
            }
            public override string ToString() => "<native fn insert>";
        }

        private class NativeArrayRemove : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object?> arguments)
            {
                List<object> list = null;
                if (arguments[0] is QArray qArray) list = qArray.Elements;
                else if (arguments[0] is List<object> l) list = l;

                if (list != null)
                {
                    int index = Convert.ToInt32(arguments[1]);
                    if (index >= 0 && index < list.Count)
                    {
                        object removed = list[index];
                        list.RemoveAt(index);
                        return removed;
                    }
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Index out of bounds.");
                }
                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "Expected array as first argument.");
            }
            public override string ToString() => "<native fn remove>";
        }
    }
}


