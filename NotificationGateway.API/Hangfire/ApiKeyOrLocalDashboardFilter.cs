using System.Net;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace NotificationGateway.API.Hangfire;

public sealed class ApiKeyOrLocalDashboardFilter : IDashboardAuthorizationFilter
{
    public const string ApiKeyHeaderName = "X-Hangfire-Key";

    private readonly byte[]? _expectedKeyBytes;
    private readonly bool _allowLoopback;

    public ApiKeyOrLocalDashboardFilter(string? apiKey, bool allowLoopback)
    {
        _expectedKeyBytes = string.IsNullOrEmpty(apiKey) ? null : Encoding.UTF8.GetBytes(apiKey);
        _allowLoopback = allowLoopback;
    }

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var header = http.Request.Headers.TryGetValue(ApiKeyHeaderName, out var value)
            ? value.ToString()
            : null;

        return ShouldAllow(http.Connection.RemoteIpAddress, header, _expectedKeyBytes, _allowLoopback);
    }

    internal static bool ShouldAllow(IPAddress? remoteIp, string? providedApiKey, byte[]? expectedKeyBytes, bool allowLoopback)
    {
        if (allowLoopback && remoteIp is not null && IPAddress.IsLoopback(remoteIp))
            return true;

        if (expectedKeyBytes is null || string.IsNullOrEmpty(providedApiKey))
            return false;

        var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);
        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedKeyBytes);
    }
}
