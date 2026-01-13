using System;
using System.Collections.Generic;
using Quartz.Parsing;

namespace Quartz.Runtime.Types
{
    internal class QStructInstance
    {
        public QStruct Template { get; }
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        internal IReadOnlyDictionary<string, object> Fields => fields;

        public QStructInstance(QStruct template)
        {
            Template = template;
            
            foreach (var field in template.Fields)
            {
                fields[field.Name.Value] = GetDefaultValue(field.Type.Type);
            }
        }

        private object GetDefaultValue(TokenType type)
        {
            switch (type)
            {
                case TokenType.Int: return 0;
                case TokenType.Double: return 0.0;
                case TokenType.Bool: return false;
                case TokenType.StringType: return "";
                case TokenType.Pointer: return new QPointer(0);
                default: return 0; 
            }
        }

        public object Get(Token name)
        {
            if (fields.TryGetValue(name.Value, out var value))
            {
                return value;
            }

            if (Template.Methods != null && Template.Methods.TryGetValue(name.Value, out var method))
            {
                return method.Bind(this);
            }

            throw new Exception($"Undefined field or method '{name.Value}' in struct {Template.Name}.");
        }

        public void Set(Token name, object value)
        {
            if (fields.ContainsKey(name.Value))
            {
                fields[name.Value] = value;
            }
            else
            {
                throw new Exception($"Cannot add new fields to struct instance {Template.Name}.");
            }
        }

        public override string ToString() => $"<struct instance {Template.Name}>";
    }
}

