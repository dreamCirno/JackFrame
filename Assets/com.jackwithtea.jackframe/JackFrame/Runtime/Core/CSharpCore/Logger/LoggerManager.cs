using System;
using System.Collections.Generic;
using UnityEngine;

namespace JackFrame {

    public enum LogMessageType {
        Log,
        Warning,
        Error,
        Exception,
        Assert
    }

    public class LoggerManager : ILoggerManager {

        LoggerSystemBase consoleLogger = new UnityLoggerSystem();
        List<LoggerSystemBase> combineList = new List<LoggerSystemBase>();

        LoggerSystemBase unityFileLogger;

        bool isPauseConsoleLogger = false;

        public void AddLoggerSystem(LoggerSystemBase loggerSystem) {

            if (loggerSystem.LoggerType == LoggerType.UnityLogFile) {
                // consoleLogger.Log("特殊情况： UnityLogFile不需要Add，只需要New即可");
                unityFileLogger = loggerSystem;
                return;
            }

            if (!combineList.Contains(loggerSystem)) {
                combineList.Add(loggerSystem);
            }

        }

        public void PauseConsoleLogger(bool isPause) {
            isPauseConsoleLogger = isPause;
        }

        public void Log(string message) {

            if (!isPauseConsoleLogger) {
                consoleLogger.Log(message);
            }

            combineList.ForeachBack(log => {
                log.Log(message);
            });

        }

        public void Warning(string message) {

            if (!isPauseConsoleLogger) {
                consoleLogger.Warning(message);
            }

            combineList.ForeachBack(log => {
                log.Warning(message);
            });

        }

        public void Error(string message) {

            if (!isPauseConsoleLogger) {
                consoleLogger.Error(message);
            }

            combineList.ForeachBack(log => {
                log.Error(message);
            });

        }

        public void Assert(bool isConditionTrueThenPass, string message) {

            if (!isPauseConsoleLogger) {
                consoleLogger.Assert(isConditionTrueThenPass, message);
            }

            combineList.ForeachBack(log => {
                log.Assert(isConditionTrueThenPass, message);
            });

        }

        public void TearDown() {

            unityFileLogger?.Destroy();

            combineList.ForeachBack(log => {
                log.Destroy();
            });

        }

    }

}