using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Kafka.LinqToKafka;

public class KafkaContext(IHttpClientFactory httpClientFactory, IOptions<KafkaQueryOptions> options)
    : IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly string _baseUrl = options?.Value?.BaseUrl ?? throw new ArgumentNullException(nameof(options));
    private readonly ConcurrentDictionary<Type, object> _sets = new();
    private bool _disposed;

    protected KafkaSet<TEntity> Set<TEntity>() where TEntity : class
    {
        var httpClient = _httpClientFactory.CreateClient();
        var queryable = new KafkaQueryable<TEntity>(httpClient, _baseUrl);
        
        return new(queryable);
    }

    protected virtual KafkaSet<TEntity> GetSet<TEntity>() where TEntity : class
    {
        return (KafkaSet<TEntity>)_sets.GetOrAdd(typeof(TEntity), t => Set<TEntity>());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}