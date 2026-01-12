using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Interfaces;

namespace Quartz.Runtime.Types
{
    internal class QClass : ICallable
    {
        public string Name { get; }
        private readonly Dictionary<string, Function> methods;

        public QClass(string name, Dictionary<string, Function> methods)
        {
            Name = name;
            this.methods = methods;
        }

        public Function FindMethod(string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Arity()
        {
            Function initializer = FindMethod("init");
            if (initializer == null) return 0;
            return initializer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            QInstance instance = new QInstance(this);
            Function initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }
    }
}

