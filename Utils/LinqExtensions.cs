using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.Utils
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T element in source) act(element);
            return source;
        }
    }
}
