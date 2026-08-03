using AnotadorGymApp.Api.Features.Entrenamiento;
using AnotadorGymApp.Api.Features.Entrenamiento.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnotadorGymAppApi.Features.Entrenamiento
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Usuario")]
    public class EntrenamientoController : ControllerBase
    {
        private readonly IEntrenamientoService _service;
        private readonly ILogger<EntrenamientoController> _logger;
        private int UsuarioId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public EntrenamientoController(IEntrenamientoService service, ILogger<EntrenamientoController> logger)
        {
            _service = service;
            _logger = logger;            
        }

        /// <summary>
        /// Crea un nuevo Entrenamiento (agregado completo).
        /// El servicio devuelve el id creado.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] EntrenamientoDto dto, CancellationToken cancellationToken)
        {            
            var id = await _service.CrearAsync(dto, UsuarioId, cancellationToken);

            return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
        }

        /// <summary>
        /// Obtiene un Entrenamiento completo por id (incluye ejercicios y series).
        /// </summary>
        [HttpGet("entrenamiento/{id:int}")]
        public async Task<ActionResult<EntrenamientoDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            var ent = await _service.ObtenerPorIdAsync(id, UsuarioId, cancellationToken);

            if (ent is null) return NotFound();

            return Ok(ent);
        }

        /// <summary>
        /// Obtiene todos los entrenamientos de un usuario.
        /// Ruta: GET /api/entrenamientos/mis-entrenamientos/{usuarioId}
        /// </summary>
        [HttpGet("mis-entrenamientos")]
        public async Task<ActionResult<IEnumerable<EntrenamientoDto>>> ObtenerPorUsuario(CancellationToken cancellationToken)
        {
            var list = await _service.ObtenerPorUsuarioAsync(UsuarioId, cancellationToken);
                                    
            return Ok(list);
        }

        /// <summary>
        /// Sincroniza/actualiza un Entrenamiento completo. El servicio debe comparar y aplicar insert/update/delete
        /// sobre los hijos (EjercicioEntrenado, SerieEntrenada) manteniendo al Entrenamiento como aggregate root.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Sincronizar(int id, [FromBody] EntrenamientoDto dto, CancellationToken cancellationToken)
        {            
            var ok = await _service.SincronizarEntrenamiento(id, dto , cancellationToken);

            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Elimina un Entrenamiento (cascade configurada en el modelo).
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {            
            var ok = await _service.BorrarAsync(UsuarioId, id, cancellationToken);

            if (!ok) return NotFound();
            return NoContent();
        }        
    }    
}   
