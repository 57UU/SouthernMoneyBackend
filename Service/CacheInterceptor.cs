using Castle.DynamicProxy;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Service;

public class CacheInterceptor : IInterceptor
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public CacheInterceptor(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public void Intercept(IInvocation invocation)
    {
        var cachedAttr = invocation.Method.GetCustomAttributes(typeof(CachedAttribute), false)
            .FirstOrDefault() as CachedAttribute;

        if (cachedAttr == null)
        {
            invocation.Proceed();
            return;
        }

        var key = BuildCacheKey(cachedAttr.Key, invocation.Arguments);
        var expiry = ParseExpiry(cachedAttr.Expiry);

        // Try to get from cache
        var cachedValue = _db.StringGetAsync(key).GetAwaiter().GetResult();
        if (cachedValue.HasValue)
        {
            var returnType = invocation.Method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = returnType.GetGenericArguments()[0];
                var jsonString = cachedValue.ToString();
                var result = JsonSerializer.Deserialize(jsonString, innerType);
                var taskType = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(innerType);
                invocation.ReturnValue = taskType.Invoke(null, new[] { result })!;
                return;
            }
        }

        // Proceed with original method
        invocation.Proceed();

        // Cache the result (if not void)
        var returnValue = invocation.ReturnValue;
        if (returnValue is Task task)
        {
            var continuation = task.ContinueWith(async _ =>
            {
                var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
                var result = typeof(Task<>).MakeGenericType(resultType)
                    .GetProperty(nameof(Task<object>.Result))!.GetValue(task);
                if (result != null)
                {
                    var json = JsonSerializer.Serialize(result);
                    await _db.StringSetAsync(key, json, expiry);
                }
            });
            return;
        }
    }

    private string BuildCacheKey(string pattern, object[] args)
    {
        var result = pattern;
        for (int i = 0; i < args.Length; i++)
        {
            result = result.Replace($"{{{i}}}", args[i]?.ToString() ?? "null");
        }
        return result;
    }

    private TimeSpan ParseExpiry(string expiry)
    {
        var match = Regex.Match(expiry, @"(\d+)([smh])");
        if (!match.Success) return TimeSpan.FromMinutes(10);

        var value = int.Parse(match.Groups[1].Value);
        return match.Groups[2].Value switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => TimeSpan.FromMinutes(10)
        };
    }
}