using System;
using System.Collections.Generic;
using Quartz.Interfaces;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Modules
{
    internal class ConverterModule : Module
    {
        public ConverterModule() : base(new Environment())
        {
            ExportedEnv.Define("toInt", new ToIntFunction());
            ExportedEnv.Define("toDouble", new ToDoubleFunction());
            ExportedEnv.Define("toString", new ToStringFunction());
            ExportedEnv.Define("byteToChar", new ByteToCharFunction());
        }

        private class ByteToCharFunction : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] is int i) return ((char)i).ToString();
                if (arguments[0] is double d) return ((char)(int)d).ToString();
                if (arguments[0] is long l) return ((char)(int)l).ToString();
                return "";
            }
        }

        private class ToIntFunction : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] == null) return 0;
                if (arguments[0] is int i) return i;
                if (arguments[0] is double d) return (int)d;
                if (arguments[0] is string s)
                {
                    if (int.TryParse(s, out int result)) return result;
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "String could not be converted to int.");
                }
                if (arguments[0] is bool b) return b ? 1 : 0;

                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Cannot convert {arguments[0].GetType().Name} to int.");
            }
        }

        private class ToDoubleFunction : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] == null) return 0.0;
                if (arguments[0] is double d) return d;
                if (arguments[0] is int i) return (double)i;
                if (arguments[0] is string s)
                {
                    if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result)) return result;
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), "String could not be converted to double.");
                }
                if (arguments[0] is bool b) return b ? 1.0 : 0.0;

                throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Cannot convert {arguments[0].GetType().Name} to double.");
            }
        }

        private class ToStringFunction : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] == null) return "null";
                return arguments[0].ToString();
            }
        }
    }
}


