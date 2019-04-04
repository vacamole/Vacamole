using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Team6.Engine.MemoryManagement
{
    public class ObjectPool<T> : ObjectPool where T : class, IDisposable
    {
        private readonly List<T> availableObjects;
        private SpinLock spinLock = new SpinLock();

        private Func<T> factory;

        public ObjectPool(Func<T> factory, int initialCapacity = 128)
        {
            this.factory = factory;
            this.availableObjects = new List<T>(initialCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PooledObject<T> GetFree()
        {
            T result;
            bool gotLock = false;
            try
            {
                spinLock.TryEnter(ref gotLock);
                if (gotLock && availableObjects.Count > 0)
                {
                    result = availableObjects[availableObjects.Count - 1];
                    availableObjects.RemoveAt(availableObjects.Count - 1);
                }
                else
                    result = factory();
            }
            finally
            {
                if (gotLock)
                    spinLock.Exit();
            }
            return new PooledObject<T>(result, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFree(T value)
        {
            bool gotLock = false;
            try
            {
                spinLock.Enter(ref gotLock);
#if DEBUG
                if (!gotLock)
                    throw new Exception("Spinlock failed");
#endif
                availableObjects.Add(value);
            }
            finally
            {
                if (gotLock)
                    spinLock.Exit();
            }
        }
    }

    public class ObjectPool
    {

    }

    public struct PooledObject<T> : IDisposable where T : class, IDisposable
    {
        private ObjectPool<T> objectPool;

        public PooledObject(T val, ObjectPool<T> objectPool)
        {
            this.Value = val;
            this.objectPool = objectPool;
        }

        public T Value { get; private set; }

        public void Dispose()
        {
            Free();
        }

        public void Free()
        {
            if (Value != null)
                objectPool.SetFree(Value);
            Value = null;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return Value == obj;
        }
    }
}