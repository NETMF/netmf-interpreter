namespace System
{
    [Serializable()]
    public class OutOfMemoryException : SystemException
    {
        public OutOfMemoryException()
            : base()
        {
        }

        public OutOfMemoryException(String message)
            : base(message)
        {
        }

        public OutOfMemoryException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


