using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class NativeFunc : ICallable
    {
        private readonly Func<Interpreter, List<object>, object> CallDelegate;

        private readonly Func<int> ArityDelegate;

        public NativeFunc(Func<Interpreter, List<object>, object> callDelegate, Func<int> arityDelegate)
        {
            CallDelegate = callDelegate;
            ArityDelegate = arityDelegate;
        }

        public object Call(Interpreter interpreter, List<object> args) => CallDelegate(interpreter, args);

        public int Arity() => ArityDelegate();

        public override string ToString() => "<native fn>";
    }
}
