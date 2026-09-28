using NotificationGateway.Domain.Security;
using System.Net;
using Xunit;

namespace NotificationGateway.Domain.Tests.Security
{
    public class UrlSafetyTests
    {
        [Theory]
        [InlineData("https://example.com/webhook", true)]
        [InlineData("http://example.com/webhook", true)]
        [InlineData("https://hooks.slack.com/services/T00000/B00000/XXXX", true)]
        [InlineData("http://example.com", true)]
        public void AllowsPublicHttpHttpsUrls(string url, bool expected)
        {
            Assert.Equal(expected, UrlSafety.IsSafeWebhookTarget(url));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("/relative/path")]
        [InlineData("not a url")]
        [InlineData("ftp://example.com/file")]
        [InlineData("file:///etc/passwd")]
        [InlineData("gopher://example.com")]
        public void RejectsNonHttpAbsoluteTargets(string? url)
        {
            Assert.False(UrlSafety.IsSafeWebhookTarget(url));
        }

        [Theory]
        [InlineData("http://localhost/webhook")]
        [InlineData("http://LOCALHOST:8080/x")]
        [InlineData("http://myhost.local/x")]
        [InlineData("http://svc.internal/x")]
        [InlineData("http://metadata/x")]
        [InlineData("http://metadata.google.internal/x")]
        public void RejectsInternalHostnames(string url)
        {
            Assert.False(UrlSafety.IsSafeWebhookTarget(url));
        }

        [Theory]
        [InlineData("http://127.0.0.1/x")]
        [InlineData("http://0.0.0.0/x")]
        [InlineData("http://10.0.0.5/x")]
        [InlineData("http://172.16.0.1/x")]
        [InlineData("http://192.168.1.1/x")]
        [InlineData("http://169.254.169.254/latest/meta-data/")]
        [InlineData("http://100.64.0.1/x")]
        [InlineData("http://[::1]/x")]
        [InlineData("http://[fe80::1]/x")]
        [InlineData("http://[fc00::1]/x")]
        [InlineData("http://[::]/x")]
        public void RejectsPrivateIpLiteralTargets(string url)
        {
            Assert.False(UrlSafety.IsSafeWebhookTarget(url));
        }

        [Theory]
        [InlineData("http://8.8.8.8/x")]
        [InlineData("http://93.184.216.34/x")]
        public void AllowsPublicIpLiteralTargets(string url)
        {
            Assert.True(UrlSafety.IsSafeWebhookTarget(url));
        }

        [Theory]
        [InlineData("127.0.0.1", true)]
        [InlineData("10.1.2.3", true)]
        [InlineData("172.20.0.1", true)]
        [InlineData("192.168.0.1", true)]
        [InlineData("169.254.1.1", true)]
        [InlineData("100.64.0.1", true)]
        [InlineData("0.0.0.0", true)]
        [InlineData("8.8.8.8", false)]
        [InlineData("172.15.0.1", false)]
        [InlineData("192.169.0.1", false)]
        public void IsPrivateAddress_CoversIpv4Ranges(string ip, bool expectedPrivate)
        {
            Assert.Equal(expectedPrivate, UrlSafety.IsPrivateAddress(IPAddress.Parse(ip)));
        }

        [Fact]
        public void IsPrivateAddress_TreatsMappedIpv6AsPrivate()
        {
            var mapped = IPAddress.Parse("::ffff:127.0.0.1");
            Assert.True(UrlSafety.IsPrivateAddress(mapped));
        }

        [Fact]
        public void IsPrivateAddress_NullIsTreatedAsPrivate()
        {
            Assert.True(UrlSafety.IsPrivateAddress(null));
        }

        [Theory]
        [InlineData("127.0.0.1", true)]
        [InlineData("[::1]", true)]
        [InlineData("example.com", false)]
        public void IsIpAddressLiteral_RecognizesV4AndBracketedV6(string host, bool expected)
        {
            Assert.Equal(expected, UrlSafety.IsIpAddressLiteral(host));
        }
    }
}
