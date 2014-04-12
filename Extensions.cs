using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Extensions
{
    public static T MinBy<T, K>(this IEnumerable<T> source, Func<T, K> selector)
    {
        var comparer = Comparer<K>.Default;
        
        var min = source.First();
        var minV = selector(min);

        foreach (var s in source)
        {
            var v = selector(s);
            if (comparer.Compare(v, minV) < 0)
            {
                minV = v;
                min = s;
            }
        }

        return min;
    }
}
