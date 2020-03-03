using System;
using System.Collections.Generic;

namespace AuxiliaryLibraries.Extensions
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<T[]> SplitExclude<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return enumerable.SplitBase(predicate, false, true);
        }

        public static IEnumerable<T[]> SplitInclude<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool insertToStart)
        {
            return enumerable.SplitBase(predicate, true, insertToStart);
        }

        static IEnumerable<T[]> SplitBase<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool include, bool first)
        {
            //List<T[]> returned = new List<T[]>();

            List<T> temp = new List<T>();

            foreach (var a in enumerable)
            {
                if (predicate.Invoke(a))
                {
                    if (include)
                    {
                        if (first)
                        {
                            if (temp.Count > 0)
                                yield return temp.ToArray();                        
                            temp.Clear();
                            temp.Add(a);
                        }
                        else
                        {
                            temp.Add(a);
                            yield return temp.ToArray();
                            temp.Clear();
                        }
                    }
                    else
                    {
                        if (temp.Count > 0)
                            yield return temp.ToArray();
                        temp.Clear();
                    }
                }
                else
                    temp.Add(a);
            }
            if (temp.Count > 0)
                yield return temp.ToArray();
        }
    }
}