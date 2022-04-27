using System;
using System.IO;
using System.Collections.Generic;

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

        public static bool IsBindingLog;
        public static bool IsBindingWarning;
        public static bool IsBindingError;
        public static bool IsBindingAssert;
        public static Action<string> OnLog;
        public static Action<string> OnWarning;
        public static Action<string> OnError;
        public static Action<bool, string> OnAssert;
        public static Action<bool> OnAssertWithoutMessage;
        public static Action OnTearDown;

        static List<string> cacheList = new List<string>();

        static bool isFirstFileLog = false;

        public static LogLevel Level { get; set; } = LogLevel.All;

        public static void Log(string message) {

            if ((Level & LogLevel.Log) == 0) {
                return;
            }

            if (OnLog != null) {
                OnLog.Invoke(message);
            } else {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("OnLog 未注册");
#else
                System.Console.WriteLine("OnLog 未注册");
#endif
            }

        }

        public static void LogUnregisterHandle(string nameofHandle) {
            const string PREFIX = "未注册";
            Log(PREFIX + " " + nameofHandle);
        }

        public static void Warning(string message) {

            if ((Level & LogLevel.Warning) == 0) {
                return;
            }

            if (OnWarning != null) {
                OnWarning.Invoke(message);
            } else {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("OnWarning 未注册");
#else
                System.Console.WriteLine("OnWarning 未注册");
#endif
            }

        }

        public static void Error(string message) {

            if ((Level & LogLevel.Error) == 0) {
                return;
            }

            if (OnError != null) {
                OnError.Invoke(message);
            } else {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("OnError 未注册");
#else
                System.Console.WriteLine("OnError 未注册");
#endif
            }

        }

        public static void Assert(bool isCondition) {
            if (OnAssertWithoutMessage != null) {
                OnAssertWithoutMessage.Invoke(isCondition);
            } else {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("OnAssertWithoutMessage 未注册");
#else
                System.Console.WriteLine("OnAssertWithoutMessage 未注册");
#endif
            }
        }

        public static void Assert(bool isCondition, string message) {
            if (OnAssert != null) {
                OnAssert.Invoke(isCondition, message);
            } else {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("OnAssert 未注册");
#else
                System.Console.WriteLine("OnAssert 未注册");
#endif
            }
        }

        public static void ForceLog(string message) {

#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#else
            System.Console.WriteLine(message);
#endif
        }

        public static void ForceWarning(string message) {

#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message);
#else
            System.Console.WriteLine("[warn]" + message);
#endif
        }

        public static void ForceError(string message) {

#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            System.Console.WriteLine("[err]" + message);
#endif
        }

        public static void ForceAssert(bool condition) {
#if UNITY_EDITOR
            UnityEngine.Debug.Assert(condition);
#else
            System.Diagnostics.Debug.Assert(condition);
#endif
        }

        public static void ForceAssert(bool condition, string message) {
#if UNITY_EDITOR
            UnityEngine.Debug.Assert(condition, message);
#else
            System.Diagnostics.Debug.Assert(condition, message);
#endif
        }

        public static void AddToFile(string log, string filePath = "DefaultLog.log", LogLevel logLevel = LogLevel.Log) {
            if (!isFirstFileLog) {
                string hr = "======================================== 新的片段 ========================================";
                IniHelper.AddAndSave(filePath, DateTime.Now.ToLocalTime().ToString(), hr);
                isFirstFileLog = true;
            }
            IniHelper.AddAndSave(filePath, DateTime.Now.ToLocalTime().ToString(), "[" + logLevel.ToString() + "]" + log);
        }

        public static void AddToCache(string log) {
            string str = DateTime.Now.ToLocalTime().ToString() + ": " + log;
            cacheList.Add(str);
        }

        public static void SaveCacheToFile(string filePath = "DefaultLog.log") {
            if (cacheList.Count == 0) {
                return;
            }
            if (!File.Exists(filePath)) {
                return;
            }
            string log = "";
            for (int i = 0; i < cacheList.Count; i += 1) {
                log += cacheList[i] + "\r\n";
            }
            FileHelper.SaveFileText(log, filePath);
        }

        public static void TearDown() {
            if (OnTearDown != null) {
                OnTearDown.Invoke();
            }
        }

    }

}