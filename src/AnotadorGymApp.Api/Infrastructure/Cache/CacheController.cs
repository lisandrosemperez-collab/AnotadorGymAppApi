using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnotadorGymAppApi.Infrastructure.Cache
{
    [Authorize(Roles = "Admin")]
    [Route("api/cache")]
    public class CacheController : Controller
    {

        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;
        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }
        
        [HttpPost("borrar")]
        public async Task<IActionResult> BorrarTodo()
        {
            await _cacheService.DeleteAsync("Ejercicios.json");
            await _cacheService.DeleteAsync("Rutinas.json");
            _logger.LogInformation("Cache borrada por el usuario {User}", User.Identity?.Name);
            return Ok();
        }               
    }
}
