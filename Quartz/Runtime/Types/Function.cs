using Quartz.AST;
using Quartz.Runtime;
using Quartz.Interfaces;
using Quartz.Exceptions;
using System.Collections.Generic;

namespace Quartz.Runtime.Types
{
    internal class Function : ICallable
    {
        private readonly string name;
        private readonly List<Quartz.Parsing.Token> parameters;
        private readonly List<Stmt> body;
        private readonly Environment closure;
        private readonly FunctionStmt declaration; 
        

        public Function(FunctionStmt declaration, Environment closure)
        {
            this.name = declaration.Name.Value;
            this.parameters = declaration.Params;
            this.body = declaration.Body;
            this.closure = closure;
            this.declaration = declaration;
        }

        public Function(string name, List<Quartz.Parsing.Token> parameters, List<Stmt> body, Environment closure)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
            this.closure = closure;
            this.declaration = null;
        }

        public int Arity() => parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < parameters.Count; i++)
            {
                environment.Define(parameters[i].Value, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(this.body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }

            return null;
        }

        public override string ToString() => $"<fn {name}>";

        public Function Bind(QInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new Function(name, parameters, body, environment);
        }

        public Function Bind(QStructInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new Function(name, parameters, body, environment);
        }
    }
}


