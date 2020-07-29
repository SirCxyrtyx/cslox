using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    internal class Environment
    {
        public readonly Environment Enclosing;
        readonly Dictionary<string, object> Values = new Dictionary<string, object>();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

        public void Define(string name, object value) => Values[name] = value;

        public object Get(Token name)
        {
            if (Values.TryGetValue(name.Lexeme, out object value))
            {
                return value;
            }

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined varable '{name.Lexeme}'!");
        }

        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values[name.Lexeme] = value;
            }
            else if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }
            throw new RuntimeError(name, $"Undefined varable '{name.Lexeme}'!");
        }
    }
}
