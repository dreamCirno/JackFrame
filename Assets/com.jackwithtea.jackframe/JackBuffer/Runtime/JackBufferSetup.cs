using System;

namespace JackBuffer {

    public static class JackBufferSetup {

        public static int MaxBufferSize { get; private set; } = 1024;

        static JackBufferSetup() {
        }

        public static void SetMaxBufferSize(int maxBufferSize) {
            MaxBufferSize = maxBufferSize;
        }

    }

}