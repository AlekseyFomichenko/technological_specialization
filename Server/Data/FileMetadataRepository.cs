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

        public async Task<IReadOnlyList<FileMetadata>> GetByReceiverLoginAsync(string receiverLogin, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata
                .Where(f => f.ReceiverLogin == receiverLogin)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FileMetadata>> GetUndeliveredForUserAsync(string receiverLogin, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata
                .Where(f => f.ReceiverLogin == receiverLogin && !f.IsDelivered)
                .OrderBy(f => f.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateDeliveredAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var file = await _context.FileMetadata.FindAsync([fileId], cancellationToken);
            if (file is null)
                return false;
            file.IsDelivered = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
