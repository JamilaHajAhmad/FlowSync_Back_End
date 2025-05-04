using Microsoft.Extensions.Caching.Memory;

namespace WebApplicationFlowSync.services.CacheServices
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Cache<T>(string key, T? value)
        {
            if (object.Equals(value, default(T)))
                _cache.Remove(key);
            else
            {
                //Add default MemoryCacheEntryOptions
                var options = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High);
                _cache.Set(key, value, options);
            }
        }

        public T? GetObject<T>(string key)
        {
            var value = _cache.Get<T>(key);
            return value;
        }
    }

 }
