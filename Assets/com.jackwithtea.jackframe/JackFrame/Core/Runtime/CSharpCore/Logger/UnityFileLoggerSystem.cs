using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace JackFrame {

    public class UnityFileLoggerSystem : LoggerSystemBase {

        public override LoggerType LoggerType => LoggerType.UnityLogFile;

        string path;
        bool isUseCache;
        StringBuilder sb;

        public UnityFileLoggerSystem(string dir, bool isUseCache) {

            this.path = @dir + "/" + GetFileDate() + ".txt";
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            this.isUseCache = isUseCache;
            if (isUseCache) {
                sb = new StringBuilder();
            }

            Application.logMessageReceived += OnLog;

        }

        void OnLog(string condition, string track, LogType logType) {
            switch(logType) {
                case LogType.Log: Log(condition + "\r\n" + track); break;
                case LogType.Assert: Log(condition + "\r\n" + track); break;
                case LogType.Error: Error(condition + "\r\n" + track); break;
                case LogType.Warning: Warning(condition + "\r\n" + track); break;
                case LogType.Exception: Log(condition + "\r\n" + track); break;
            }
        }

        public override void Assert(bool isConditionTrueThenPass, string message) {
            if (!isConditionTrueThenPass) {
                message = GetDate() + "[Assert]" + message;
                Record(message);
            }
        }

        public override void Log(string message) {
            if ((PLog.Level & PLog.LogLevel.Log) == 0) {
                return;
            }
            message = GetDate() + "<Log>" + message;
            Record(message);
        }

        public override void Error(string message) {
            if ((PLog.Level & PLog.LogLevel.Error) == 0) {
                return;
            }
            message = GetDate() + "<Error>" + message;
            Record(message);
        }

        public override void Warning(string message) {
            if ((PLog.Level & PLog.LogLevel.Warning) == 0) {
                return;
            }
            message = GetDate() + "<Warning>" + message;
            Record(message);
        }

        public override void Destroy() {
            if (sb != null) {
                using(StreamWriter file = new StreamWriter(path, true)) {
                    file.Write(sb.ToString());
                }
            }
        }

        void Record(string message) {
            if (isUseCache) {
                sb.AppendLine(message);
            } else {
                using(StreamWriter sw = new StreamWriter(path, true)) {
                    sw.WriteLine(message);
                }
            }
        }

        string GetFileDate() {
            var now = DateTime.Now.ToLocalTime();
            string year = now.Year.ToString();
            string month = now.Month.ToString();
            string day = now.Day.ToString();
            string hour = now.Hour.ToString();
            string minute = now.Minute.ToString();
            string second = now.Second.ToString();
            return @"[" + year + "," + month + "," + day + "]" + hour + "." + minute + "." + second;
        }

        string GetDate() {
            return DateTime.UtcNow.ToLocalTime().ToString() + " ";
        }
    }
}