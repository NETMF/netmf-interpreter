namespace System
{
    [Serializable]
    public class ArgumentNullException : ArgumentException
    {
        public ArgumentNullException() : base() { }
        public ArgumentNullException(string argument) : base(null, argument) { }
        public ArgumentNullException(String paramName, String message) : base(message, paramName) { }
    }
}


