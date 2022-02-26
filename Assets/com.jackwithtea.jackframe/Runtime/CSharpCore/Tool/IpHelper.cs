using System;
using System.Net;
using System.Net.Sockets;

namespace JackFrame {

    public static class IpHelper {

        public static string GetLocalIpAddress() {
            IPHostEntry entry;
            try {
                entry = Dns.GetHostEntry(Dns.GetHostName());
            } catch(Exception) {
                entry = Dns.GetHostEntry("localhost");
            }
            string ipv4 = "";
            for (int i = 0; i < entry.AddressList.Length; i += 1) {
                IPAddress address = entry.AddressList[i];
                if (address.AddressFamily == AddressFamily.InterNetwork && !address.IsIPv6LinkLocal) {
                    ipv4 = address.ToString();
                    break;
                }
            }
            return ipv4;
        }

    }
}