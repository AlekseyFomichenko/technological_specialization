namespace Shared.Models
{
    public enum MessageType : byte
    {
        Register = 0x01,
        Login = 0x02,
        TextMessage = 0x03,
        FileStart = 0x04,
        FileChunk = 0x05,
        FileEnd = 0x06,
        Error = 0x07,
        Ack = 0x08,
        FileStartAck = 0x09
    }
}
