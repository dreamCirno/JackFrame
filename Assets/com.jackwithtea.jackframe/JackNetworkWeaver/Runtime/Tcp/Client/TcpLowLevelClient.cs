using System;
using Telepathy;
using JackFrame;

namespace JackFrame.Network {

    public class TcpLowLevelClient {

        Client client;
        string host;
        int port;

        public event Action OnConnectedHandle;
        public event Action<ArraySegment<byte>> OnDataHandle;
        public event Action OnDisconnectedHandle;

        public TcpLowLevelClient(int maxMessageSize) {
            client = new Client(maxMessageSize);
            client.OnConnected += OnConnected;
            client.OnData += OnData;
            client.OnDisconnected += OnDisconnected;
        }

        public bool IsConnected() {
            return client.Connected;
        }

        public void Connect(string host, int port) {
            this.host = host;
            this.port = port;
            client.Connect(host, port);
        }

        public void Reconnect() {
            if (IsConnected()) {
                client.Disconnect();
            }
            Connect(host, port);
        }

        public void Disconnect() {
            client.Disconnect();
        }

        public void Tick(int processLimit = 100) {
            client.Tick(processLimit);
        }

        public void TearDown() {

        }

        public bool Send(ArraySegment<byte> data) {
            return client.Send(data);
        }

        void OnConnected() {
            if (OnConnectedHandle != null) {
                OnConnectedHandle.Invoke();
            } else {
                PLog.ForceWarning("未注册: " + nameof(OnConnectedHandle));
            }
        }

        void OnData(ArraySegment<byte> data) {
            if (OnDataHandle != null) {
                OnDataHandle.Invoke(data);
            } else {
                PLog.ForceError("未注册: " + nameof(OnDataHandle));
                return;
            }
        }

        void OnDisconnected() {
            if (OnDisconnectedHandle != null) {
                OnDisconnectedHandle.Invoke();
            } else {
                PLog.ForceWarning("未注册: " + nameof(OnDisconnectedHandle));
            }
        }

    }

}