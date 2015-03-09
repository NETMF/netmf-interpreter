namespace System
{
    [Serializable()]
    public class AppDomainUnloadedException : SystemException
    {
        public AppDomainUnloadedException()
            : base()
        {
        }

        public AppDomainUnloadedException(String message)
            : base(message)
        {
        }

        public AppDomainUnloadedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


