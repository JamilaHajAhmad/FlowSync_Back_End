namespace WebApplicationFlowSync.services.CacheServices
{
    public interface ICacheService
    {
        void Cache<T>(string key, T? value);
        T? GetObject<T>(string key);
    }
}
