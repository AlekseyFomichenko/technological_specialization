namespace Server.Services.Abstracts
{
    internal interface ITokenValidator
    {
        Task<string?> ValidateAsync(string token, CancellationToken cancellationToken = default);
    }
}
