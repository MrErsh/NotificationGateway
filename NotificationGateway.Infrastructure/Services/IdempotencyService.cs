using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(IDistributedCache cache, ILogger<IdempotencyService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task StoreResponseAsync<TResponse>(string idempotencyKey, TResponse response, TimeSpan expiry)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return;

            try
            {
                var serializedResponse = System.Text.Json.JsonSerializer.Serialize(response);
                await _cache.SetStringAsync(
                    idempotencyKey,
                    serializedResponse,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry },
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store idempotency response for key {IdempotencyKey}", idempotencyKey);
            }
        }

        public async Task<TResponse?> GetResponseAsync<TResponse>(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return default;

            try
            {
                var cachedResponse = await _cache.GetStringAsync(idempotencyKey, cancellationToken);
                if (cachedResponse == null)
                    return default;

                return System.Text.Json.JsonSerializer.Deserialize<TResponse>(cachedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve idempotency response for key {IdempotencyKey}", idempotencyKey);
                return default;
            }
        }
    }
}
