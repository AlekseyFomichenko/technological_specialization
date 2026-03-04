using Shared.DTO;

namespace Server.Services.Abstracts
{
    internal interface IFileTransferService
    {
        Task<StartFileResult> StartReceivingAsync(Guid connectionId, string senderLogin, FileStartPayload payload, CancellationToken cancellationToken = default);
        Task<WriteChunkResult> WriteChunkAsync(Guid connectionId, ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken = default);
        Task<EndFileResult> EndReceivingAsync(Guid connectionId, CancellationToken cancellationToken = default);
        Task CancelReceivingAsync(Guid connectionId, CancellationToken cancellationToken = default);
        Task DeliverPendingFilesForUserAsync(string login, CancellationToken cancellationToken = default);
    }
}
