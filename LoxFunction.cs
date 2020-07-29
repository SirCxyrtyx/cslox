using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class LoxFunction : ICallable
    {
        private readonly Function Declaration;
        private readonly Environment Closure;

        public LoxFunction(Function declaration, Environment closure)
        {
            Declaration = declaration;
            Closure = closure;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            var env = new Environment(Closure);
            foreach ((Token param, object arg) in Declaration.Parameters.Zip(args))
            {
                env.Define(param.Lexeme, arg);
            }

            try
            {
                interpreter.ExecuteBlock(Declaration.Body, env);
            }
            catch (Return returnVal)
            {
                return returnVal.Value;
            }
            return null;
        }

        public int Arity() => Declaration.Parameters.Count;

        public override string ToString() => $"<fn {Declaration.Name.Lexeme}>";
    }
}
