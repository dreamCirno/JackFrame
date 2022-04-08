using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace AnymotionNS {

    public class StackPool<T> {

        Stack<T> stack;

        Func<T> CreateFunc;

        public StackPool(int poolSize, Func<T> createFunc) {
            this.stack = new Stack<T>(poolSize);
            this.CreateFunc = createFunc;
            // Add range to stack
            for (int i = 0; i < poolSize; i += 1) {
                stack.Push(createFunc());
            }
        }

        public T Take() {
            if (stack.Count > 0) {
                return stack.Pop();
            } else {
                return CreateFunc();
            }
        }

        public void Return(T obj) {
            stack.Push(obj);
        }

        public void Clear() {
            stack.Clear();
        }

    }
}