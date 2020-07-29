using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTool
{
    internal static class Extensions
    {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> kvp, out T key, out U value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
