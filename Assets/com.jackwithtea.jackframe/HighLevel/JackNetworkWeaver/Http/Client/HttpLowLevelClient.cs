using System.Text;
using System.Net.Http;

namespace JackFrame.Network {

    public class HttpLowLevelClient {

        HttpClient client;

        public HttpLowLevelClient() {
            this.client = new HttpClient();
        }

        public void SetDomain(string domain) {
            client.BaseAddress = new System.Uri(domain);
        }

        public void PostStringAsync(string uri, string msg) {
            ByteArrayContent byteArray = new ByteArrayContent(Encoding.UTF8.GetBytes(msg));
            client.PostAsync(uri, byteArray);
        }

    }

}