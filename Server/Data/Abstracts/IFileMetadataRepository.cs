using Server.Models;

namespace Server.Data.Abstracts
{
    internal interface IFileMetadataRepository
    {
        Task AddAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
        Task<FileMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FileMetadata>> GetByReceiverIdAsync(Guid receiverId, CancellationToken cancellationToken = default);
    }
}
