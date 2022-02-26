using System;
using System.Collections.Generic;

namespace JackFrame {

    public abstract class LoggerSystemBase {

        public abstract LoggerType LoggerType { get; }

        protected LoggerSystemBase() {}

        public abstract void Log(string message);
        public abstract void Warning(string message);
        public abstract void Error(string message);
        public abstract void Assert(bool isConditionTrueThenPass, string message);
        public abstract void Destroy();

    }

}