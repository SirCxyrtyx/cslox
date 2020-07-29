using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    internal static class Extensions
    {
        public static string Slice(this string str, int start, int end) => str.Substring(start, end - start);
    }
}
