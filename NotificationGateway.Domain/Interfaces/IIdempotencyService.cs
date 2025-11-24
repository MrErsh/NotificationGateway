namespace NotificationGateway.Domain.Interfaces
{
    public interface IIdempotencyService
    {
        Task StoreResponseAsync<TResponse>(string idempotencyKey, TResponse response, TimeSpan expiry);
        Task<TResponse?> GetResponseAsync<TResponse>(string idempotencyKey, CancellationToken cancellationToken = default);
    }
}
