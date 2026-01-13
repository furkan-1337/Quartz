using Quartz.Parsing;
using System.Collections.Generic;

namespace Quartz.Runtime.Types
{
    public class QEnum
    {
        public string Name { get; }
        private readonly Dictionary<string, object> members = new Dictionary<string, object>();

        public QEnum(string name, Dictionary<string, object> members)
        {
            Name = name;
            this.members = members;
        }

        public object Get(Token name)
        {
            if (members.TryGetValue(name.Value, out object value))
            {
                return value;
            }

            throw new System.Exception($"Enum '{Name}' does not contain member '{name.Value}'.");
        }

        public override string ToString()
        {
            return $"<enum {Name}>";
        }
    }
}

