using System.Collections.Generic;
using Quartz.AST;
using Quartz.Interfaces;

namespace Quartz.Runtime.Types
{
    internal class QStruct : ICallable
    {
        public string Name { get; }
        public List<StructField> Fields { get; }
        public Dictionary<string, Function> Methods { get; }

        public QStruct(string name, List<StructField> fields, Dictionary<string, Function> methods)
        {
            Name = name;
            Fields = fields;
            Methods = methods;
        }

        public int Arity()
        {
            if (Methods != null && Methods.TryGetValue("init", out var initializer))
            {
                return initializer.Arity();
            }
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            QStructInstance instance = new QStructInstance(this);
            if (Methods != null && Methods.TryGetValue("init", out var initializer))
            {
                Function initMethod = initializer.Bind(instance);
                initMethod.Call(interpreter, arguments);
            }
            return instance;
        }

        public override string ToString() => $"<struct {Name}>";
    }
}

