using Quartz.AST;
using Quartz.Runtime;
using Quartz.Interfaces;
using Quartz.Exceptions;
using System.Collections.Generic;

namespace Quartz.Runtime.Types
{
    internal class Function : ICallable
    {
        private readonly FunctionStmt declaration;
        private readonly Environment closure;

        public Function(FunctionStmt declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public int Arity() => declaration.Params.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.Define(declaration.Params[i].Value, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }

            return null;
        }

        public override string ToString() => $"<fn {declaration.Name.Value}>";

        public Function Bind(QInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new Function(declaration, environment);
        }
    }
}

