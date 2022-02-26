using System;
using System.Threading;
using System.Threading.Tasks;

namespace JackFrame {

    public static class ThreadExtention {

        public static string ToState(this Thread thread) {
            string s = $"线程id: {thread.ManagedThreadId.ToString()}, 是否主线程: {(!thread.IsBackground).ToString()}, 线程状态: {thread.ThreadState.ToString()}";
            return s;
        }

    }

}