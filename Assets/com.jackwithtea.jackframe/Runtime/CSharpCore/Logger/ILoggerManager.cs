using System.Collections.Generic;

namespace JackFrame {
    public interface ILoggerManager {
        void AddLoggerSystem(LoggerSystemBase loggerSystem);
        void Log(string message);
        void Warning(string message);
        void Error(string message);
        void Assert(bool isConditionTrueThenPass, string message);
        void PauseConsoleLogger(bool isPause);
        void TearDown();
    }

}