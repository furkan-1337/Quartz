using System;
using System.Collections.Generic;
using System.Text;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Modules
{
    internal class StringModule : Module
    {
        public StringModule() : base(new Environment())
        {
            ExportedEnv.Define("length", new NativeLength());
            ExportedEnv.Define("upper", new NativeUpper());
            ExportedEnv.Define("lower", new NativeLower());
            ExportedEnv.Define("substring", new NativeSubstring());
            ExportedEnv.Define("replace", new NativeReplace());
            ExportedEnv.Define("split", new NativeSplit());
        }

        private class NativeLength : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                return str.Length;
            }
            public override string ToString() => "<native fn length>";
        }

        private class NativeUpper : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                return str.ToUpper();
            }
            public override string ToString() => "<native fn upper>";
        }

        private class NativeLower : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                return str.ToLower();
            }
            public override string ToString() => "<native fn lower>";
        }

        private class NativeSubstring : ICallable
        {
            public int Arity() => 3;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                int start = Convert.ToInt32(arguments[1]);
                int length = Convert.ToInt32(arguments[2]);
                if (start < 0 || start >= str.Length || start + length > str.Length)
                {
                    throw new Exception("Index out of bounds.");
                }
                return str.Substring(start, length);
            }
            public override string ToString() => "<native fn substring>";
        }

        private class NativeReplace : ICallable
        {
            public int Arity() => 3;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                string oldV = Convert.ToString(arguments[1]);
                string newV = Convert.ToString(arguments[2]);
                return str.Replace(oldV, newV);
            }
            public override string ToString() => "<native fn replace>";
        }

        private class NativeSplit : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string str = Convert.ToString(arguments[0]);
                string sep = Convert.ToString(arguments[1]);

                string[] parts = str.Split(new string[] { sep }, StringSplitOptions.None);

                // Convert string[] to List<object>
                List<object> list = new List<object>();
                foreach (var part in parts)
                {
                    list.Add(part);
                }
                return list;
            }
            public override string ToString() => "<native fn split>";
        }
    }
}

