namespace Shared.Protocol
{
    /// <summary>
    /// Исключение при нарушении протокола (неизвестный тип сообщения, неверная длина payload и т.п.).
    /// </summary>
    public sealed class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message) { }

        public ProtocolException(string message, Exception innerException) : base(message, innerException) { }
    }
}
