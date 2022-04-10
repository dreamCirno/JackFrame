using System;
using System.Collections.Generic;

namespace JackFrame {

    public class Pool<T> {

        Stack<T> pool;

        Func<T> generateHandle;

        public Pool(Func<T> generateHandle, int originSize) {
            this.generateHandle = generateHandle;
            this.pool = new Stack<T>(originSize);
            for (int i = 0; i < originSize; i += 1) {
                pool.Push(generateHandle.Invoke());
            }
        }

        public T Take() {
            if (pool.Count > 0) {
                return pool.Pop();
            } else {
                return generateHandle.Invoke();
            }
        }

        public void Return(T obj) {
            pool.Push(obj);
        }

    }
}