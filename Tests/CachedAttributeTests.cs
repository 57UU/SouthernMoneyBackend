using Service;
using Xunit;

namespace Tests;

public class CachedAttributeTests
{
    [Fact]
    public void CachedAttribute_DefaultValues()
    {
        var attr = new CachedAttribute();

        Assert.Equal("", attr.Key);
        Assert.Equal("10m", attr.Expiry);
    }

    [Fact]
    public void CachedAttribute_CanSetProperties()
    {
        var attr = new CachedAttribute
        {
            Key = "user:{0}",
            Expiry = "2h"
        };

        Assert.Equal("user:{0}", attr.Key);
        Assert.Equal("2h", attr.Expiry);
    }

    [Fact]
    public void CachedAttribute_CanBeAppliedToMethod()
    {
        var method = typeof(TestService).GetMethod(nameof(TestService.GetData));
        var attr = method!.GetCustomAttributes(typeof(CachedAttribute), false)
            .FirstOrDefault() as CachedAttribute;

        Assert.NotNull(attr);
        Assert.Equal("test:data", attr.Key);
        Assert.Equal("5m", attr.Expiry);
    }

    // Helper class for testing attribute application
    public class TestService
    {
        [Cached(Key = "test:data", Expiry = "5m")]
        public void GetData() { }
    }
}