using System;
using System.Collections.Generic;

namespace JackFrame {

    public static class QueueExtention {

        public static bool TryDequeue<T>(this Queue<T> q, out T value) {
            if (q.Count > 0) {
                value = q.Dequeue();
                return true;
            } else {
                value = default;
                return false;
            }
        }
    }
}