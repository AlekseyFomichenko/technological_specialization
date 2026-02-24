namespace Server.Services
{
    internal static class AuthErrorCodes
    {
        public const string LoginRequired = "LOGIN_REQUIRED";
        public const string PasswordRequired = "PASSWORD_REQUIRED";
        public const string LoginTooShort = "LOGIN_TOO_SHORT";
        public const string PasswordTooWeak = "PASSWORD_TOO_WEAK";
        public const string LoginTaken = "LOGIN_TAKEN";
        public const string Blocked = "BLOCKED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
    }
}
