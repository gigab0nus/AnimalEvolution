using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    static class LinkedListHelper
    {
        public static T MaxObject<T>(this LinkedList<T> list, Func<T, T, T> comparator)
        {
            T highest = list.First();
            foreach (T t in list)
            {
                highest = comparator(t, highest);
            }
            return highest;
        }

        public static T FirstWhere<T>(this LinkedList<T> list, Func<T, bool> predicate)
        {
            foreach(T t in list)
            {
                if (predicate(t))
                    return t;
            }
            return default(T);
        }
    }
}
