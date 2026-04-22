using Castle.DynamicProxy;
using StackExchange.Redis;

namespace Service;

public class CacheProxyGenerator
{
    private readonly ProxyGenerator _generator;
    private readonly IConnectionMultiplexer _redis;

    public CacheProxyGenerator(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _generator = new ProxyGenerator();
    }

    public T CreateCachedProxy<T>(T target) where T : class
    {
        var interceptor = new CacheInterceptor(_redis);
        return _generator.CreateClassProxyWithTarget(target, interceptor);
    }
}