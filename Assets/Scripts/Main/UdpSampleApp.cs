using System;
using UnityEngine;
using JackFrame.Network;

namespace MySampleApp {

    public class UdpSampleApp : MonoBehaviour {

        UdpLowLevelClient client;
        UdpLowLevelServer server;

        void Awake() {

            server = new UdpLowLevelServer();
            server.OnConnectedHandle += (connID) => {
                Debug.Log("SERVER CONN: " + connID);
            };
            server.OnDisconnectedHandle += (connID) => {
                Debug.Log("SERVER DISCONN: " + connID);
            };
            server.OnRecvDataHandle += (connID, data) => {
                Debug.Log("SERVER RECV: " + connID + ", " + data.Length);
            };
            server.StartListen(5000);

            client = new UdpLowLevelClient();
            client.OnConnectedHandle += () => {
                Debug.Log("CLIENT CONN");
                client.Send(new byte[] { 2, 3, 4, 5, 6 });
            };
            client.OnDisconnectedHandle += () => {
                Debug.Log("CLIENT DISCONN");
            };
            client.OnRecvDataHandle += (data) => {
                Debug.Log("CLIENT RECV: " + data.Length);
            };
            client.Connect("localhost", 5000, "key");

        }

        void Update() {
            server.Tick();
            client.Tick();
        }

    }

}