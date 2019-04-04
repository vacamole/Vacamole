using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.MemoryManagement;

namespace Team6.Engine.Misc
{
    /// <summary>
    /// Maintains a set of elements that is splitted by a key into subsets. Those subsets can be efficiently and easily accessed.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys</typeparam>
    /// <typeparam name="TElement">The type of the elements</typeparam>
    public class MultiKeyedSets<TKey, TElement> : IEnumerable<TElement>
    {
        private Dictionary<TKey, HashSet<TElement>> elements = new Dictionary<TKey, HashSet<TElement>>();
        private HashSet<TElement> flatList = new HashSet<TElement>();
        private Func<TElement, IEnumerable<TKey>> keySelector;

        private static readonly HashSet<TElement> emptySet = new HashSet<TElement>();

        public MultiKeyedSets(Func<TElement, IEnumerable<TKey>> keySelector)
        {
            this.keySelector = keySelector;
        }

        public bool Add(TElement element)
        {
            bool result = flatList.Add(element);
            if (result)
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (TKey key in keySelector(element))
                {
                    if (!elements.ContainsKey(key))
                        elements.Add(key, new HashSet<TElement>());

                    elements[key].Add(element);
                }
            }

            return result;
        }

        public bool Remove(TElement element)
        {
            bool result = flatList.Remove(element);

            if (result)
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (TKey key in keySelector(element))
                {
                    HashSet<TElement> setForKey = elements[key];
                    setForKey.Remove(element);
                    if (setForKey.Count == 0)
                        elements.Remove(key);
                }
            }

            return result;
        }

        public SafeHashSetEnumerable<TElement> GetAll(TKey key)
        {
            HashSet<TElement> list;

            return elements.TryGetValue(key, out list) ? list.ToSaveEnumerable() : emptySet.ToSaveEnumerable();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            throw new NotSupportedException("Don't create garbage");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("Don't create garbage");
        }
    }
}
