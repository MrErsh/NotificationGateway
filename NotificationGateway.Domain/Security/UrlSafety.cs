using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NotificationGateway.Domain.Security;

public static class UrlSafety
{
    private static readonly HashSet<string> BlockedHostnames = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "ip6-localhost",
        "ip6-loopback",
        "metadata",
        "metadata.google.internal",
        "instance-data",
    };

    public static bool IsSafeWebhookTarget(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var host = uri.Host;
        if (string.IsNullOrEmpty(host))
            return false;

        if (BlockedHostnames.Contains(host))
            return false;

        if (host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        if (IsIpAddressLiteral(host, out var ip))
            return !IsPrivateAddress(ip);

        return true;
    }

    public static bool IsIpAddressLiteral(string host)
        => IsIpAddressLiteral(host, out _);

    public static bool IsIpAddressLiteral(string host, out IPAddress address)
    {
        if (host.Length >= 2 && host[0] == '[' && host[^1] == ']')
            host = host[1..^1];

        return IPAddress.TryParse(host, out address!);
    }

    public static bool IsPrivateAddress(IPAddress? address)
    {
        if (address is null)
            return true;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return true;

        if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            return true;

        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => IsPrivateIPv4(address),
            AddressFamily.InterNetworkV6 => IsPrivateIPv6(address),
            _ => true,
        };
    }

    private static bool IsPrivateIPv4(IPAddress address)
    {
        var b = address.GetAddressBytes();
        if (b.Length != 4)
            return true;

        if (b[0] == 0) return true;
        if (b[0] == 10) return true;
        if (b[0] == 100 && (b[1] & 0xC0) == 64) return true;
        if (b[0] == 127) return true;
        if (b[0] == 169 && b[1] == 254) return true;
        if (b[0] == 172 && (b[1] & 0xF0) == 16) return true;
        if (b[0] == 192 && b[1] == 0 && (b[2] == 0 || b[2] == 2)) return true;
        if (b[0] == 192 && b[1] == 168) return true;
        if (b[0] == 198 && (b[1] == 18 || b[1] == 19)) return true;
        if (b[0] == 198 && b[1] == 51 && b[2] == 100) return true;
        if (b[0] == 203 && b[1] == 0 && b[2] == 113) return true;
        if ((b[0] & 0xF0) == 0xE0) return true;
        if (b[0] >= 240) return true;

        return false;
    }

    private static bool IsPrivateIPv6(IPAddress address)
    {
        var b = address.GetAddressBytes();
        if (b.Length != 16)
            return true;

        if (b[0] == 0xFE && (b[1] & 0xC0) == 0x80) return true;
        if ((b[0] & 0xFE) == 0xFC) return true;
        if (b[0] == 0xFF) return true;
        if (b[0] == 0x20 && b[1] == 0x01 && b[2] == 0x0D && b[3] == 0xB8) return true;
        if (b[0] == 0x20 && b[1] == 0x01 && b[2] == 0x00 && b[3] == 0x00) return true;

        return false;
    }
}
