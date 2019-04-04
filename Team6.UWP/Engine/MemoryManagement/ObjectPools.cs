using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.MemoryManagement
{
    public static class ObjectPools
    {
        static readonly ConcurrentDictionary<Type, ObjectPool> objectPools = new ConcurrentDictionary<Type, ObjectPool>();

        public static ObjectPool<T> CreateOrGet<T>(Func<T> factory) where T : class, IDisposable
        {
            ObjectPool pool = objectPools.GetOrAdd(typeof(T), (t) => CreatePool<T>(factory));
            return (ObjectPool<T>)pool;
        }

        private static ObjectPool CreatePool<T>(Func<T> factory) where T : class, IDisposable
        {
            return new ObjectPool<T>(factory);
        }

        public static ObjectPool<T> Get<T>() where T : class, IDisposable
        {
            ObjectPool pool;
            if (objectPools.TryGetValue(typeof(T), out pool))
                return (ObjectPool<T>)pool;
            throw new KeyNotFoundException();
        }
    }
}
