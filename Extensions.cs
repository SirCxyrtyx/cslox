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

        public static IEnumerable<(T, U)> Zip<T, U>(this IEnumerable<T> first, IEnumerable<U> second)
        {
            return first.Zip(second, (t, u) => (t, u));
        }
    }
}
