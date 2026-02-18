namespace ChatTransport.Abstracts
{
    public interface IFileTransferSender
    {
        Task SendAsync(string host, int port, Stream content, long length, CancellationToken cancellationToken = default);
    }
}
