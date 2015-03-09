namespace System
{
    [Serializable()]
    public class IndexOutOfRangeException : SystemException
    {
        public IndexOutOfRangeException()
            : base()
        {
        }

        public IndexOutOfRangeException(String message)
            : base(message)
        {
        }

        public IndexOutOfRangeException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


