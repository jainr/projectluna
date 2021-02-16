namespace Luna.Clients.Exceptions
{
    public class LunaNotSupportedUserException : LunaUserException
    {
        public LunaNotSupportedUserException(string message):
            base(message, UserErrorCode.NotSupported, System.Net.HttpStatusCode.NotImplemented)
        {

        }
    }
}
