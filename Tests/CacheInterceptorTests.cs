using Service;
using Castle.DynamicProxy;
using StackExchange.Redis;
using Moq;
using Xunit;

namespace Tests;

public class CacheInterceptorTests
{
    [Theory]
    [InlineData("user:{0}", new object[] { 123 }, "user:123")]
    [InlineData("posts:page:{0}:{1}", new object[] { 1, 10 }, "posts:page:1:10")]
    [InlineData("key:{0}:{1}", new object[] { "foo", "bar" }, "key:foo:bar")]
    [InlineData("no_params", new object[] { }, "no_params")]
    [InlineData("user:{0}", new object[] { null }, "user:null")]
    public void BuildCacheKey_ReplacesPlaceholders(string pattern, object[] args, string expected)
    {
        // Use reflection to test private method
        var method = typeof(CacheInterceptor).GetMethod("BuildCacheKey",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var redisMock = new Mock<IConnectionMultiplexer>();
        var dbMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        var interceptor = new CacheInterceptor(redisMock.Object);
        var result = method.Invoke(interceptor, new object[] { pattern, args }) as string;

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("10s", 10, "seconds")]
    [InlineData("30m", 30, "minutes")]
    [InlineData("2h", 2, "hours")]
    [InlineData("1m", 1, "minutes")]
    public void ParseExpiry_ReturnsCorrectTimeSpan(string expiry, int value, string unit)
    {
        var method = typeof(CacheInterceptor).GetMethod("ParseExpiry",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var redisMock = new Mock<IConnectionMultiplexer>();
        var dbMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        var interceptor = new CacheInterceptor(redisMock.Object);
        var result = method.Invoke(interceptor, new object[] { expiry }) as TimeSpan?;

        Assert.NotNull(result);
        var expected = unit switch
        {
            "seconds" => TimeSpan.FromSeconds(value),
            "minutes" => TimeSpan.FromMinutes(value),
            "hours" => TimeSpan.FromHours(value),
            _ => TimeSpan.FromMinutes(value)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseExpiry_InvalidFormat_ReturnsDefault()
    {
        var method = typeof(CacheInterceptor).GetMethod("ParseExpiry",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var redisMock = new Mock<IConnectionMultiplexer>();
        var dbMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        var interceptor = new CacheInterceptor(redisMock.Object);
        var result = method.Invoke(interceptor, new object[] { "invalid" }) as TimeSpan?;

        Assert.Equal(TimeSpan.FromMinutes(10), result);
    }
}