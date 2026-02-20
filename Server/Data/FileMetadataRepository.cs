using Microsoft.EntityFrameworkCore;
using Server.Data.Abstracts;
using Server.Models;

namespace Server.Data
{
    internal class FileMetadataRepository : IFileMetadataRepository
    {
        private readonly ChatDbContext _context;

        public FileMetadataRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
        {
            await _context.FileMetadata.AddAsync(fileMetadata, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<FileMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata.FindAsync([id], cancellationToken);
        }

        public async Task<IReadOnlyList<FileMetadata>> GetByReceiverIdAsync(Guid receiverId, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata
                .Where(f => f.ReceiverId == receiverId)
                .ToListAsync(cancellationToken);
        }
    }
}
