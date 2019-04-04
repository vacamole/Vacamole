using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.MemoryManagement
{
    /// <summary>
    /// A garbage free enumerator
    /// </summary>
    public struct SafeListEnumerable<T>
    {
        List<T>.Enumerator listEnumerator;

        public List<T>.Enumerator GetEnumerator()
        {
            return listEnumerator;
        }

        public SafeListEnumerable(List<T> list)
        {
            listEnumerator = list.GetEnumerator();
        }
    }

    /// <summary>
    /// A garbage free enumerator
    /// </summary>
    public struct SafeHashSetEnumerable<T>
    {
        HashSet<T> set;

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return set.GetEnumerator();
        }

        public SafeHashSetEnumerable(HashSet<T> set)
        {
            this.set = set;
        }

        public int Count
        {
            get { return set.Count; }
        }
    }


    public struct SafeSelectHashSetEnumerable<TSelected, TSource>
    {
        private readonly SafeHashSetEnumerable<TSource> source;
        private readonly Func<TSource, TSelected> select;

        public SafeSelectHashSetEnumerable(SafeHashSetEnumerable<TSource> source, Func<TSource, TSelected> select)
        {
            this.source = source;
            this.select = select;
        }

        public SafeSelectHashSetEnumerator<TSelected, TSource> GetEnumerator()
        {
            return new SafeSelectHashSetEnumerator<TSelected, TSource>(source, select);
        }

        public int Count
        {
            get { return source.Count; }
        }
    }

    public struct SafeSelectHashSetEnumerator<TSelected, TSource> : IEnumerator<TSelected>
    {
        private HashSet<TSource>.Enumerator source;
        private readonly Func<TSource, TSelected> select;

        public SafeSelectHashSetEnumerator(SafeHashSetEnumerable<TSource> source, Func<TSource, TSelected> select)
        {
            this.source = source.GetEnumerator();

            this.select = select;
            this.Current = default(TSelected);
        }

        public TSelected Current { get; private set; }

        object IEnumerator.Current
        {
            get { throw new NotSupportedException("Don't generate garbage"); }
        }

        public void Dispose()
        {
            source.Dispose();
        }

        public bool MoveNext()
        {
            bool result = source.MoveNext();
            var item = source.Current;
            Current = result ? select(item) : default(TSelected);
            return result;
        }

        public void Reset()
        {
        }
    }


    public static class SafeEnumerableEx
    {
        public static SafeListEnumerable<T> ToSaveEnumerable<T>(this List<T> list)
        {
            return new SafeListEnumerable<T>(list);
        }

        public static SafeHashSetEnumerable<T> ToSaveEnumerable<T>(this HashSet<T> set)
        {
            return new SafeHashSetEnumerable<T>(set);
        }

        public static SafeSelectHashSetEnumerable<TSelected, TSource> Cast<TSelected, TSource>(this SafeHashSetEnumerable<TSource> source) where TSelected : class
        {
            return new SafeSelectHashSetEnumerable<TSelected, TSource>(source, val =>
            {
                object notBoxedValue = val;
                return (TSelected)notBoxedValue;
            });
        }

        public static List<T> ToList<T>(this SafeHashSetEnumerable<T> set)
        {
            List<T> result = new List<T>(set.Count);

            foreach (T element in set)
                result.Add(element);

            return result;
        }

        public static List<TSelected> ToList<TSelected, TSource>(
            this SafeSelectHashSetEnumerable<TSelected, TSource> set)
        {
            List<TSelected> result = new List<TSelected>(set.Count);

            foreach (TSelected element in set)
                result.Add(element);

            return result;
        }

        public static void ForEach<T>(this SafeHashSetEnumerable<T> enumerable, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in enumerable)
                action(elem);
        }

        public static void ForEach<T, TS>(this SafeSelectHashSetEnumerable<T, TS> enumerable, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in enumerable)
                action(elem);
        }

        public static void ForEach<T>(this SafeListEnumerable<T> enumerable, Action<T> action)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (T elem in enumerable)
                action(elem);
        }

        public static T FirstOrDefault<T, TS>(this SafeSelectHashSetEnumerable<T, TS> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;
            else
                return default(T);
        }
    }
}
