using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class ListExt
    {
        public static T AddAndReturn<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return item;
        }
    }
}
