using System.Collections.Generic;

namespace CSharpLox
{
    public class LoxInstance
    {
        private readonly LoxClass Class;
        private readonly Dictionary<string, object> Fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass loxClass)
        {
            Class = loxClass;
        }

        public override string ToString() => $"{Class.Name} instance";

        public object Get(Token name)
        {
            if (Fields.TryGetValue(name.Lexeme, out object value))
            {
                return value;
            }

            LoxFunction method = Class.FindMethod(name.Lexeme);
            if (method != null)
            {
                return method.Bind(this);
            }

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'!");
        }

        public void Set(Token name, object value)
        {
            Fields[name.Lexeme] = value;
        }
    }
}