using AnotadorGymApp.Api.Features.Usuarios.Results;
using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnotadorGymApp.Api.Features.Usuarios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,User")]
    public class UsuarioController : Controller
    {
        private int UsuarioId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private readonly IJwtProvider _jwtProvider;
        private readonly IUsuarioService _usuarioService;
        public UsuarioController(IJwtProvider jwtProvider, IUsuarioService usuarioService)
        {
            _jwtProvider = jwtProvider;
            _usuarioService = usuarioService;
        }

        [HttpGet("rutina-activa")]
        public async Task<IActionResult> ObtenerRutinaActiva()
        {
            var rutinaId = await _usuarioService.ObtenerRutinaActiva(UsuarioId);

            if (rutinaId == null)
                return NotFound();

            return Ok(rutinaId);
        }

        [HttpPut("rutina-activa/{rutinaId:int}")]
        public async Task<IActionResult> GuardarRutina(int rutinaId)
        {
            var result = await _usuarioService.GuardarRutinaActiva(UsuarioId, rutinaId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return NoContent();
        }
    }
}
