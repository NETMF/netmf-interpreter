using System;
using Ws.Services;
using Ws.Services.Xml;
using Dpws.Device.Services;

namespace Interop.SimpleService
{
    class SimpleDeviceHost : DpwsHostedService
    {
        public SimpleDeviceHost(ProtocolVersion version) : base(version)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("sim", "http://schemas.example.org/SimpleService");
            ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
            ServiceTypeName = "SimpleDeviceType";
        }

        public SimpleDeviceHost() : 
            this(new ProtocolVersion10())
        {
        }
    }
}
