
namespace System
{
    [Serializable()]
    public class ApplicationException : Exception
    {
        public ApplicationException()
            : base()
        {
        }

        public ApplicationException(String message)
            : base(message)
        {
        }

        public ApplicationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


