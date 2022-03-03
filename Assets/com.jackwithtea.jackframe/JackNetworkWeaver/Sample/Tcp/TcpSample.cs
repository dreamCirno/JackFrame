using System.Collections;
using UnityEngine;

namespace JackFrame.Network.Sample {

    public class TcpSample : MonoBehaviour {

        const int PORT = 9205;

        TcpClient client;
        TcpServer server;

        void Awake() {

            server = new TcpServer();
            server.StartListen(PORT);

            client = new TcpClient();
            client.Connect("127.0.0.1", PORT);

            server.On(1, 1, () => new MyModel(), (connId, msg) => {
                PLog.ForceLog("SERVER RECV: " + msg.intValue);
            });

        }

        void Start() {
            StartCoroutine(FakeInputIE());
        }

        void Update() {

            server.Tick();

            if (client.IsConnected()) {
                client.Tick();
            }

        }

        IEnumerator FakeInputIE() {
            WaitForSeconds seconds = new WaitForSeconds(1f);
            while (enabled) {
                if (client.IsConnected()) {
                    client.Send(1, 1, new MyModel { intValue = Random.Range(0, 100) });
                }
                yield return seconds;
            }
        }

    }

}