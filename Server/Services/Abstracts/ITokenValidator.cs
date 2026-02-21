namespace Server.Services.Abstracts
{
    internal interface ITokenValidator
    {
        Task<Guid?> ValidateAsync(string token, CancellationToken cancellationToken = default);
    }
}
