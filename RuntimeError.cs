using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class RuntimeError : Exception
    {
        public readonly Token Token;

        public RuntimeError(Token token, string msg) : base(msg)
        {
            Token = token;
        }
    }
}
