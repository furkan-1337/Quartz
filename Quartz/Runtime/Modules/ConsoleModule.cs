using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Interfaces;
using Quartz.Runtime;

namespace Quartz.Runtime.Modules
{
    internal class ConsoleModule : Module
    {
        public ConsoleModule() : base(new Environment())
        {
            this.ExportedEnv.Define("print", new PrintFunction());
            this.ExportedEnv.Define("writeLine", new PrintFunction()); // Alias for consistency
            this.ExportedEnv.Define("write", new WriteFunction());
            this.ExportedEnv.Define("readLine", new ReadLineFunction());
            this.ExportedEnv.Define("clear", new ClearFunction());
        }

        private class PrintFunction : ICallable
        {
            public int Arity() => -1; // Variadic

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                Console.WriteLine(string.Join(" ", arguments));
                return null;
            }

            public override string ToString() => "<native fn print>";
        }

        private class WriteFunction : ICallable
        {
            public int Arity() => -1; // Variadic

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                Console.Write(string.Join("", arguments));
                return null;
            }

            public override string ToString() => "<native fn write>";
        }

        private class ReadLineFunction : ICallable
        {
            public int Arity() => 0;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Console.ReadLine();
            }

            public override string ToString() => "<native fn readLine>";
        }

        private class ClearFunction : ICallable
        {
            public int Arity() => 0;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                Console.Clear();
                return null;
            }

            public override string ToString() => "<native fn clear>";
        }
    }
}

