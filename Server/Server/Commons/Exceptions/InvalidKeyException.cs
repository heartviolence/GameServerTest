namespace Server.Commons.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException() { }
        public InvalidKeyException(string message) : base(message) { }
    }
}
