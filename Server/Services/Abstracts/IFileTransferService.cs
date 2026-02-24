using Shared.DTO;

namespace Server.Services.Abstracts
{
    internal interface IFileTransferService
    {
        Task<StartFileResult> StartReceivingAsync(Guid connectionId, Guid senderId, FileStartPayload payload, CancellationToken cancellationToken = default);
        Task<WriteChunkResult> WriteChunkAsync(Guid connectionId, ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken = default);
        Task<EndFileResult> EndReceivingAsync(Guid connectionId, FileEndPayload? payload, CancellationToken cancellationToken = default);
        Task CancelReceivingAsync(Guid connectionId, CancellationToken cancellationToken = default);
    }
}
