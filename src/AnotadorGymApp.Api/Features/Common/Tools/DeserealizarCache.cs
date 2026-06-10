using System.Text.Json;

namespace AnotadorGymApp.Api.Features.Common.Tools
{
    public class DeserealizarCache
    {
        public static List<T> DeserializarCache<T>(string cache)
        {
            
            return JsonSerializer.Deserialize<List<T>>(cache) ?? new List<T>();
            
        }
    }
}
