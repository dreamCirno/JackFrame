using System;
using UnityEngine;

namespace JackFrame {

    public sealed class UnityLoggerSystem : LoggerSystemBase {

        public override LoggerType LoggerType => LoggerType.Console;

        public override void Log(string message) {
            Debug.Log(message);
        }
        
        public override void Error(string message) {
            Debug.LogError(message);
        }

        public override void Warning(string message) {
            Debug.LogWarning(message);
        }

        public override void Assert(bool isConditionTrueThenPass, string message) {
            Debug.Assert(isConditionTrueThenPass, message);
        }

        public override void Destroy() {
        }

    }

}