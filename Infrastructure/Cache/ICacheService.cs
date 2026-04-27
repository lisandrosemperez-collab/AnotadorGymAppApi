namespace AnotadorGymAppApi.Infrastructure.Cache
{
    public interface ICacheService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string content);
        Task DeleteAsync(string key);
    }
}
