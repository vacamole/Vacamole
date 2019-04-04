using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in enumerable)
                action(elem);
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in array)
                action(elem);
        }

        public static void ForEach<T>(this LinkedList<T> linkedList, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in linkedList)
                action(elem);
        }


        public static Vector2 Sum<T>(this IEnumerable<T> enumerable, Func<T, Vector2> selector)
        {
            Vector2 result = Vector2.Zero;
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (Vector2 v in enumerable.Select(selector))
                result += v;
            return result;
        }

        public static Vector2 Average<T>(this IEnumerable<T> enumerable, Func<T, Vector2> selector)
        {
            int count = 0;
            Vector2 result = Vector2.Zero;
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (Vector2 v in enumerable.Select(selector))
            {
                result += v;
                count += 1;
            }
            return result / count;
        }
    }
}
