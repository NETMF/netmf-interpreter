namespace System
{
    [Serializable()]
    public class InvalidCastException : SystemException
    {
        public InvalidCastException()
            : base()
        {
        }

        public InvalidCastException(String message)
            : base(message)
        {
        }

        public InvalidCastException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


