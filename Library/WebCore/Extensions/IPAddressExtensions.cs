using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace WebCore
{
    /// <summary>Provides Available Port. </summary>
    public class IP
    {
        public static int AvailablePort()
        {
            Mutex mutex = new Mutex(false, "IP.AvailablePort");
            try
            {
                mutex.WaitOne();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(endPoint);
                    if (socket.LocalEndPoint is IPEndPoint ipEndPoint)
                    {
                        return ipEndPoint.Port;
                    }
                }
                return 0;
            }
            finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
        public static IEnumerable<string> Addresses()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface network in networkInterfaces)
            {
                if (network.OperationalStatus == OperationalStatus.Up)
                {
                    if (network.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }
                    if (network.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }
                    IPInterfaceProperties properties = network.GetIPProperties();
                    foreach (GatewayIPAddressInformation gInfo in properties.GatewayAddresses)
                    {
                        if (gInfo.Address.ToString().Equals("0.0.0.0", StringComparison.Ordinal))
                        {
                            continue;
                        }
                        foreach (UnicastIPAddressInformation unicastIpAddressInformation in properties.UnicastAddresses)
                        {
                            if (unicastIpAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                yield return unicastIpAddressInformation.Address.ToString();
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
    /// <summary>Provides IPAddress utility methods. </summary>
    public static class IPExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsAvailablePort(this int port) => new IPEndPoint(IPAddress.Loopback, port).IsAvailable();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static int AvailablePort(this int port) => Enumerable.Range(port, 65535).FirstOrDefault(i => new IPEndPoint(IPAddress.Loopback, i).IsAvailable());
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public static bool IsAvailable(this IPEndPoint endPoint)
        {
            var ips = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ips.GetActiveTcpListeners();
            foreach (IPEndPoint tcp in tcpListeners)
            {
                if (tcp.Port.Equals(endPoint.Port))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
