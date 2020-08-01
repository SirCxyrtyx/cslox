using System;
using System.Collections.Generic;

namespace CSharpLox
{
    public class LoxClass : ICallable
    {
        public const string ConstructorName = "init";

        public readonly string Name;
        private readonly Dictionary<string, LoxFunction> Methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            Methods = methods;
        }

        public override string ToString()
        {
            return Name;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            var instance = new LoxInstance(this);
            if (FindMethod(ConstructorName) is LoxFunction init)
            {
                init.Bind(instance).Call(interpreter, args);
            }
            return instance;
        }

        public int Arity()
        {
            if (FindMethod(ConstructorName) is LoxFunction init)
            {
                return init.Arity();
            }

            return 0;
        }

        public LoxFunction FindMethod(string name)
        {
            if (Methods.TryGetValue(name, out LoxFunction method))
            {
                return method;
            }

            return null;
        }
    }
}