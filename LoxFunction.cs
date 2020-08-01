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
        private readonly bool IsInitializer;

        public LoxFunction(Function declaration, Environment closure, bool isInitializer)
        {
            Declaration = declaration;
            Closure = closure;
            IsInitializer = isInitializer;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            var env = new Environment(Closure);
            foreach ((Token param, object arg) in Declaration.Parameters.Zip(args))
            {
                env.Define(param.Lexeme, arg);
            }

            object retVal = null;
            try
            {
                interpreter.ExecuteBlock(Declaration.Body, env);
            }
            catch (Return returnVal)
            {
                retVal = returnVal.Value;
            }

            return IsInitializer ? Closure.GetAt(0, "this") : retVal;
        }

        public int Arity() => Declaration.Parameters.Count;

        public override string ToString() => $"<fn {Declaration.Name.Lexeme}>";

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(Closure);
            environment.Define("this", instance);
            return new LoxFunction(Declaration, environment, IsInitializer);
        }
    }
}
