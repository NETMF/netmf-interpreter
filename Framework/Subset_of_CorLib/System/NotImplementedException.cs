namespace System
{
    [Serializable()]
    public class NotImplementedException : SystemException
    {
        public NotImplementedException()
            : base()
        {
        }

        public NotImplementedException(String message)
            : base(message)
        {
        }

        public NotImplementedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


