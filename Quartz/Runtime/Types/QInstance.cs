using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Parsing;

namespace Quartz.Runtime.Types
{
    internal class QInstance
    {
        private QClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public QInstance(QClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.Value))
            {
                return fields[name.Value];
            }

            Function method = klass.FindMethod(name.Value);
            if (method != null) return method.Bind(this);

            throw new Exception($"Undefined property '{name.Value}'.");
        }

        public void Set(Token name, object value)
        {
            fields[name.Value] = value;
        }

        public override string ToString()
        {
            return klass.Name + " instance";
        }
    }
}

