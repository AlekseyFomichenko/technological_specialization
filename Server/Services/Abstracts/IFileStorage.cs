namespace Server.Services.Abstracts
{
    internal interface IFileStorage
    {
        Task<string> SaveAsync(Stream source, string suggestedFileName, CancellationToken cancellationToken = default);
        Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
        Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    }
}
