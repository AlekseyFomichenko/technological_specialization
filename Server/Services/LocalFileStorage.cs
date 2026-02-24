using Microsoft.Extensions.Options;
using Server.Services.Abstracts;

namespace Server.Services
{
    internal class LocalFileStorage : IFileStorage
    {
        private readonly string _basePath;
        private readonly string[] _allowedExtensions;

        public LocalFileStorage(IOptions<FileStorageOptions> options)
        {
            var opts = options.Value;
            _basePath = Path.GetFullPath(opts.BasePath ?? string.Empty);
            _allowedExtensions = opts.AllowedExtensions ?? Array.Empty<string>();
        }

        public async Task<string> SaveAsync(Stream source, string suggestedFileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(suggestedFileName);
            if (string.IsNullOrEmpty(extension) || !Array.Exists(_allowedExtensions, e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Extension not allowed.", nameof(suggestedFileName));

            Directory.CreateDirectory(_basePath);
            var fileName = Guid.NewGuid().ToString("N") + extension;
            var fullPath = Path.Combine(_basePath, fileName);

            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await source.CopyToAsync(fileStream, cancellationToken);

            return fileName;
        }

        public Task<(Stream WriteStream, string RelativePath)> CreateWriteStreamAsync(string suggestedFileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(suggestedFileName);
            if (string.IsNullOrEmpty(extension) || !Array.Exists(_allowedExtensions, e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Extension not allowed.", nameof(suggestedFileName));

            Directory.CreateDirectory(_basePath);
            var fileName = Guid.NewGuid().ToString("N") + extension;
            var fullPath = Path.Combine(_basePath, fileName);
            var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            return Task.FromResult<(Stream, string)>((stream, fileName));
        }

        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPathAndValidate(relativePath);
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return Task.FromResult<Stream>(stream);
        }

        public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPathAndValidate(relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return Task.CompletedTask;
        }

        private string GetFullPathAndValidate(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath.Contains("..", StringComparison.Ordinal))
                throw new ArgumentException("Invalid path.", nameof(relativePath));

            var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
            var baseFull = Path.GetFullPath(_basePath);
            if (!fullPath.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Path traversal not allowed.", nameof(relativePath));

            return fullPath;
        }
    }
}
