using System;
using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// A class to manage a set of parameterized pools.  Ex: A pool of float[] buffers pooled by length.
    /// </summary>
    /// <typeparam name="T">The underlying pooled object type.  Ex: float[] for simple buffer.</typeparam>
    /// <typeparam name="TArg">The type on which the pools are parameterized.  Ex: int for array length.</typeparam>
    public sealed class MetaPool<T, TArg>
    {
        private readonly Dictionary<TArg, ConstructingPool<T>> pools;
        private readonly Func<TArg, T> itemConstructor;

        public delegate void PoolModifier(ConstructingPool<T> pool);

        public PoolModifier onPoolCreate = null;

        private ConstructingPool<T>.ItemModifier _onCreate = null;
        public ConstructingPool<T>.ItemModifier OnCreate
        {
            get => _onCreate;
            set
            {
                _onCreate = value;
                foreach (ConstructingPool<T> pool in pools.Values)
                {
                    pool.onCreate = _onCreate;
                }
            }
        }

        private ConstructingPool<T>.ItemModifier _onCheckOut = null;
        public ConstructingPool<T>.ItemModifier OnCheckOut
        {
            get => _onCheckOut;
            set
            {
                _onCheckOut = value;
                foreach (ConstructingPool<T> pool in pools.Values)
                {
                    pool.onCheckOut = _onCheckOut;
                }
            }
        }

        private ConstructingPool<T>.ItemModifier _onCheckIn = null;
        public ConstructingPool<T>.ItemModifier OnCheckIn
        {
            get => _onCheckIn;
            set
            {
                _onCheckIn = value;
                foreach (ConstructingPool<T> pool in pools.Values)
                {
                    pool.onCheckIn = _onCheckIn;
                }
            }
        }

        public MetaPool(Func<TArg, T> itemConstructor)
        {
            pools = new Dictionary<TArg, ConstructingPool<T>>();
            this.itemConstructor = itemConstructor;
        }


        public T CheckOut(TArg poolArgument)
        {
            if (!pools.ContainsKey(poolArgument))
            {
                pools.Add(poolArgument, CreatePool(poolArgument));
            }

            return pools[poolArgument].CheckOut();
        }

        public IPoolRelease<T> GetPoolRelease(TArg poolArgument)
        {
            if (!pools.ContainsKey(poolArgument))
            {
                pools.Add(poolArgument, CreatePool(poolArgument));
            }

            return pools[poolArgument];
        }

        public void CheckIn(T value)
        {
            foreach (ConstructingPool<T> pool in pools.Values)
            {
                if (pool.CheckedOutContains(value))
                {
                    //Found the pool where it belongs
                    pool.CheckIn(value);
                    return;
                }
            }

            throw new ArgumentException(
                message: $"The value {value} does not seem to belong to this MetaPool.",
                paramName: nameof(value));
        }

        public void CheckIn(T value, TArg poolArgument)
        {
            if (!pools.ContainsKey(poolArgument))
            {
                pools.Add(poolArgument, CreatePool(poolArgument));
            }

            pools[poolArgument].CheckIn(value);
        }

        private ConstructingPool<T> CreatePool(TArg poolArgument)
        {
            ConstructingPool<T> newPool = new ConstructingPool<T>(() => itemConstructor(poolArgument))
            {
                onCreate = OnCreate,
                onCheckOut = OnCheckOut,
                onCheckIn = OnCheckIn
            };

            onPoolCreate?.Invoke(newPool);

            return newPool;
        }
    }

}
