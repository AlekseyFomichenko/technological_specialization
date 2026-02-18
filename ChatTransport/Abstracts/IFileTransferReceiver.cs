namespace ChatTransport.Abstracts
{
    public interface IFileTransferReceiver
    {
        Task StartListenAsync(int port, CancellationToken cancellationToken = default);
        Task<(Stream Stream, long Length)> AcceptAsync(CancellationToken cancellationToken = default);
    }
}
