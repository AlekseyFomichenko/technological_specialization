namespace Server.Services
{
    internal static class ErrorCodes
    {
        public const string LoginRequired = "LOGIN_REQUIRED";
        public const string PasswordRequired = "PASSWORD_REQUIRED";
        public const string LoginTooShort = "LOGIN_TOO_SHORT";
        public const string PasswordTooWeak = "PASSWORD_TOO_WEAK";
        public const string LoginTaken = "LOGIN_TAKEN";
        public const string Blocked = "BLOCKED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";

        public const string ReceiverNotFound = "RECEIVER_NOT_FOUND";

        public const string FileTooLarge = "FILE_TOO_LARGE";
        public const string ExtensionNotAllowed = "EXTENSION_NOT_ALLOWED";
        public const string SizeExceeded = "SIZE_EXCEEDED";
        public const string NoActiveReceive = "NO_ACTIVE_RECEIVE";

        public const string InvalidInput = "INVALID_INPUT";
        public const string InvalidState = "INVALID_STATE";
    }
}
