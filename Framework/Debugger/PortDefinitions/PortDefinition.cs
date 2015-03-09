//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]
    public abstract class PortDefinition
    {
        protected string m_displayName;
        protected string m_port;
        protected ListDictionary m_properties = new ListDictionary();
        protected PortDefinition(string displayName, string port)
        {
            m_displayName = displayName;
            m_port = port;
        }

        public override bool Equals(object obj)
        {
            PortDefinition pd = obj as PortDefinition; if (pd == null) return false;

            return (pd.UniqueId.Equals(UniqueId));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        static public PortDefinition CreateInstanceForSerial(string displayName, string port, uint baudRate)
        {
            return new PortDefinition_Serial(displayName, port, baudRate);
        }

        static public PortDefinition CreateInstanceForUsb(string displayName, string port)
        {
            return new PortDefinition_Usb(displayName, port, new ListDictionary());
        }

        static public PortDefinition CreateInstanceForWinUsb(string displayName, string port)
        {
            return new PortDefinition_WinUsb(displayName, port, new ListDictionary());
        }

        static public PortDefinition CreateInstanceForEmulator(string displayName, string port, int pid)
        {
            return new PortDefinition_Emulator(displayName, port, pid);
        }

        static public PortDefinition CreateInstanceForTcp(IPEndPoint ipEndPoint)
        {
            return new PortDefinition_Tcp(ipEndPoint);
        }

        static public PortDefinition CreateInstanceForTcp(string name)
        {
            PortDefinition portDefinition = null;

            //From CorDebug\DebugPort.cs
            string hostName = name;
            int port = PortDefinition_Tcp.WellKnownPort;
            int portIndex = hostName.IndexOf(':');
            IPAddress address = null;

            if (portIndex > 0)
            {
                hostName = name.Substring(0, portIndex);

                if (portIndex < name.Length - 1)
                {
                    string portString = name.Substring(portIndex + 1);

                    int portT;

                    if (int.TryParse(portString, out portT))
                    {
                        port = portT;
                    }
                }
            }

            if (!IPAddress.TryParse(hostName, out address))
            {
                //Does DNS resolution make sense here?

                IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);

                if (iPHostEntry.AddressList.Length > 0)
                {
                    //choose the first one?
                    address = iPHostEntry.AddressList[0];
                }
            }

            if (address != null)
            {
                IPEndPoint ipEndPoint = new IPEndPoint(address, port);

                portDefinition = new PortDefinition_Tcp(ipEndPoint);

                //ping to see if it is alive?
            }

            return portDefinition;
        }

        static public ArrayList Enumerate(params PortFilter[] args)
        {
            ArrayList lst = new ArrayList();

            foreach (PortFilter pf in args)
            {
                PortDefinition[] res;

                switch (pf)
                {
                case PortFilter.Emulator:
                    res = Emulator.EnumeratePipes();
                    break;

                case PortFilter.Serial:
                    res = AsyncSerialStream.EnumeratePorts();
                    break;

                case PortFilter.LegacyPermiscuousWinUsb:
                case PortFilter.Usb: 
                {
                    res = WinUsb_AsyncUsbStream.EnumeratePorts( pf == PortFilter.LegacyPermiscuousWinUsb );

                    lst.AddRange(res);

                    res = AsyncUsbStream.EnumeratePorts();
                    // res will be added to list below...
                }
                    break;

                case PortFilter.TcpIp:
                    res = PortDefinition_Tcp.EnumeratePorts();
                    break;
                default: res = null; break;
                }

                if (res != null)
                {
                    lst.AddRange(res);
                }
            }

            return lst;
        }

        public string DisplayName { get { return m_displayName; } }
        public ListDictionary Properties { get { return m_properties; } }
        
        public bool TryToOpen()
        {
            bool fSuccess = false;

            try
            {
                using (Stream stream = CreateStream())
                {
                    fSuccess = true;
                    stream.Close();
                }
            }
            catch
            {
                // REVIEW: This should NOT be a general catch all!
                //         Need to determine what this is expecting
                //         and, ideallly avoid it in the first place
                //         or at least catch the specific exceptions
            }

            return fSuccess;
        }

        public virtual string Port
        {
            get
            {
                return m_port;
            }
        }

        public virtual object UniqueId
        {
            get
            {
                return m_port;
            }
        }

        public virtual string PersistName
        {
            get
            {
                return UniqueId.ToString();
            }
        }

        public virtual Stream Open()
        {
            Stream stream = CreateStream();

            return stream;
        }

        public abstract Stream CreateStream();
    }
}