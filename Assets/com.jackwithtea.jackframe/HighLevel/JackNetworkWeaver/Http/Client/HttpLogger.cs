namespace JackFrame.Network {

    public static class HttpLogger {

        static HttpLowLevelClient client = new HttpLowLevelClient();
        static string url;

        public static void SetDomain(string domain) {
            client.SetDomain(domain);
        }

        public static void SetLogUrl(string _url) {
            url = _url;
        }

        public static void Log(string msg) {
            string dst = PLog.PackLogWithDeviceInfo(PLog.LogLevel.Log, msg);
            Send(dst);
        }

        public static void Warning(string msg) {
            string dst = PLog.PackLogWithDeviceInfo(PLog.LogLevel.Warning, msg);
            Send(dst);
        }

        public static void Error(string msg) {
            string dst = PLog.PackLogWithDeviceInfo(PLog.LogLevel.Error, msg);
            Send(dst);
        }

        static void Send(string msg) {
            client.PostStringAsync(url, msg); 
        }

    }

}