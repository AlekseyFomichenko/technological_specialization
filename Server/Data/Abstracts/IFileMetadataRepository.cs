using Server.Models;

namespace Server.Data.Abstracts
{
    internal interface IFileMetadataRepository
    {
        Task AddAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
        Task<FileMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FileMetadata>> GetByReceiverLoginAsync(string receiverLogin, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FileMetadata>> GetUndeliveredForUserAsync(string receiverLogin, CancellationToken cancellationToken = default);
        Task<bool> UpdateDeliveredAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}
