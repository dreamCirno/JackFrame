using UnityEngine;
using JackFrame.Network;
using JackFrame;

namespace HttpSample {

    public class HttpSampleApp : MonoBehaviour {

        HttpLowLevelClient client;

        void Awake() {
            client = new HttpLowLevelClient();
            client.SetDomain("http://tinylog.utea.fun:5210");
            client.PostStringAsync("/log", PLog.PackLogWithDeviceInfo(PLog.LogLevel.Log, "yo"));
        }

    }

}