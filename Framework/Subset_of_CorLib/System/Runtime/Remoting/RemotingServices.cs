using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting
{
    public static class RemotingServices
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsTransparentProxy(Object proxy);
    }
}


