using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public interface ICallable
    {
        public object Call(Interpreter interpreter, List<object> args);

        public int Arity();
    }
}
