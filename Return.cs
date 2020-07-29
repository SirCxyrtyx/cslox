using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class Return : Exception
    {
        public readonly object Value;

        public Return(object value)
        {
            Value = value;
        }
    }
}
