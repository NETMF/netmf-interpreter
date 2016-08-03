using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    class Utilities
    {
        internal static string GetLocalIpAddress()
        {
            while (true)
            {
                var netifs = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var itf in netifs)
                {
                    if (itf.IPAddress == "0.0.0.0" || itf.IPAddress == "127.0.0.1")
                        continue;

                    Debug.Print("Found IP: " + itf.IPAddress);
                    return itf.IPAddress;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
