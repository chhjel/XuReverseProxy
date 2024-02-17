using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.Tests;

public class UrlUtilsTests
{
    [Theory]
    [InlineData(
        "https://some-domain.com/some/path/etc?A=1&c=2#etc",
        "http://localhost",
        "http://localhost/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "https://some-domain.com/some/path/etc?A=1&c=2#etc",
        "http://localhost:1234",
        "http://localhost:1234/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "https://some-domain.com/some/path/etc?A=1&c=2#etc",
        "localhost:1234",
        "https://localhost:1234/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "http://some-domain.com/some/path/etc?A=1&c=2#etc",
        "localhost",
        "http://localhost/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "https://some-domain.com:9999/some/path/etc?A=1&c=2#etc",
        "https://localhost:1234",
        "https://localhost:1234/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "https://some-domain.com:9999/some/path/etc?A=1&c=2#etc",
        "https://localhost",
        "https://localhost/some/path/etc?A=1&c=2#etc")]
    public void ChangeUrlAuthorityAndPort_WithValidAbsoluteVariants_ReplacesAsExpected(string url, string newAuthorityAndPort, string expectedUrl)
        => Assert.Equal(expectedUrl, UrlUtils.ChangeUrlAuthorityAndPort(url, newAuthorityAndPort));

    [Theory]
    [InlineData(
        "https://some-domain.com/some/path/etc?A=1&c=2#etc",
        "/asd",
        "https://some-domain.com/some/path/etc?A=1&c=2#etc")]
    [InlineData(
        "https://some-domain.com/some/path/etc?A=1&c=2#etc",
        "/asd/etc?etc=123#dsa",
        "https://some-domain.com/some/path/etc?A=1&c=2#etc")]
    public void ChangeUrlAuthorityAndPort_WithRelativeUrl_ReturnsOld(string url, string newAuthorityAndPort, string expectedUrl)
        => Assert.Equal(expectedUrl, UrlUtils.ChangeUrlAuthorityAndPort(url, newAuthorityAndPort));
}