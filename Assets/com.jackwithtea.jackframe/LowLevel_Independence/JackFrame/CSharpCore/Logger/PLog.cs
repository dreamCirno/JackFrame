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

        // EVENT
        public static Action<string> OnLog;
        public static Action<string> OnWarning;
        public static Action<string> OnError;
        public static Action<bool, string> OnAssert;
        public static Action<bool> OnAssertWithoutMessage;
        public static Action OnTearDown;

        // CACHE
        public static bool AllowCache = false;
        static List<string> cacheList = new List<string>();
        public static List<string> CacheList => cacheList;

        // LEVEL
        public static LogLevel Level { get; set; } = LogLevel.All;

        public static void Log(string message) {

            if ((Level & LogLevel.Log) == 0) {
                return;
            }

            if (OnLog != null) {
                OnLog.Invoke(message);
            }

            if (AllowCache) {
                AddToCache(LogLevel.Log, message);
            }

        }

        public static void Warning(string message) {

            if ((Level & LogLevel.Warning) == 0) {
                return;
            }

            if (OnWarning != null) {
                OnWarning.Invoke(message);
            }

            if (AllowCache) {
                AddToCache(LogLevel.Warning, message);
            }

        }

        public static void Error(string message) {

            if ((Level & LogLevel.Error) == 0) {
                return;
            }

            if (OnError != null) {
                OnError.Invoke(message);
            }

            if (AllowCache) {
                AddToCache(LogLevel.Error, message);
            }

        }

        public static void Assert(bool isCondition) {
            if (OnAssertWithoutMessage != null) {
                OnAssertWithoutMessage.Invoke(isCondition);
            }
        }

        public static void Assert(bool isCondition, string message) {
            if (OnAssert != null) {
                OnAssert.Invoke(isCondition, message);
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

        public static void AddToCache(LogLevel level, string log) {
            string prefix;
            switch(level) {
                case LogLevel.Log:
                    prefix = "[log]";
                    break;
                case LogLevel.Warning:
                    prefix = "[warn]";
                    break;
                case LogLevel.Error:
                    prefix = "[err]";
                    break;
                default:
                    prefix = "";
                    break;
            }

            string str = DateTime.Now.ToLocalTime().ToString() + ": " + log;
            cacheList.Add(prefix + str);
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