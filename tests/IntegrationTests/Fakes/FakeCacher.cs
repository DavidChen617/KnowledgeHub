using System.Text.Json;
using Application.Interfaces;

namespace IntegrationTests.Fakes;

public class FakeCacher : ICacher
{
    private readonly Dictionary<string, string> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_store.TryGetValue(key, out var json))
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default)
    {
        _store[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keys = _store.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var k in keys) _store.Remove(k);
        return Task.CompletedTask;
    }
}
