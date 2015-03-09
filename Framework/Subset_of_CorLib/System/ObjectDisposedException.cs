namespace System
{
    [Serializable()]
    public class ObjectDisposedException : SystemException
    {
        public ObjectDisposedException()
            : base()
        {
        }

        public ObjectDisposedException(String message)
            : base(message)
        {
        }

        public ObjectDisposedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


