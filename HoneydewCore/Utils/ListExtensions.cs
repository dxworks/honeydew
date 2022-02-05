using System.Linq;

namespace System.Collections.Generic;

public static class ListExtensions
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> otherList)
    {
        foreach (var element in otherList.ToList())
        {
            list.Add(element);
        }
    }
}
