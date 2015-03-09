using System;

namespace Microsoft.SPOT.Messaging
{
    [Serializable()]
    public class RemotedException
    {
        public string m_message;
        public RemotedException( Exception payload )
        {
            m_message = payload.Message;
        }

        public void Raise()
        {
            throw new System.Runtime.Remoting.RemotingException( m_message );
        }
    }
}