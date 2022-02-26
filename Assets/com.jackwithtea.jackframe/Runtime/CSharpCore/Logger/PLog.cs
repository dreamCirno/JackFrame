using System;

namespace JackFrame {

    /*
        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系
    */
    public static class PLog {

        [Flags]
        public enum LogLevel {
            None = 0,
            Log = 1,
            Warning = 2,
            Error = 4,
            All = Log | Warning | Error
        }

        public static event Action<string> OnLog;
        public static event Action<string> OnWarning;
        public static event Action<string> OnError;
        public static event Action<bool, string> OnAssert;
        public static event Action<bool> OnAssertWithoutMessage;

        public static LogLevel Level { get; set; } = LogLevel.All;

        public static void Log(string message) {

            if ((Level & LogLevel.Log) == 0) {
                return;
            }

            if (OnLog != null) {
                OnLog.Invoke(message);
            } else {
                throw new Exception("OnLog 未注册");
            }

        }

        public static void Warning(string message) {

            if ((Level & LogLevel.Warning) == 0) {
                return;
            }

            if (OnWarning != null) {
                OnWarning.Invoke(message);
            } else {
                throw new Exception("OnWarning 未注册");
            }

        }

        public static void Error(string message) {

            if ((Level & LogLevel.Error) == 0) {
                return;
            }

            if (OnError != null) {
                OnError.Invoke(message);
            } else {
                throw new Exception("OnError 未注册");
            }

        }

        public static void Assert(bool isCondition, string message) {
            if (OnAssert != null) {
                OnAssert.Invoke(isCondition, message);
            } else {
                throw new Exception("OnAssert 未注册");
            }
        }

        public static void Assert(bool isCondition) {
            if (OnAssertWithoutMessage != null) {
                OnAssertWithoutMessage.Invoke(isCondition);
            } else {
                throw new Exception("OnAssertWithoutMessage 未注册");
            }
        }

    }

}