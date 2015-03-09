namespace System
{
    [Serializable()]
    public class NullReferenceException : SystemException
    {
        public NullReferenceException()
            : base()
        {
        }

        public NullReferenceException(String message)
            : base(message)
        {
        }

        public NullReferenceException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


