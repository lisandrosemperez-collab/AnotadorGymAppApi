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
    [Authorize(Roles = "Admin,Usuario")]
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
            var usuarioResult = await _usuarioService.ObtenerRutinaActiva(UsuarioId);

            if (!usuarioResult.Success)
                return NotFound(usuarioResult);

            return Ok(usuarioResult);
        }

        [HttpPut("rutina-activa/{rutinaId:int}")]
        public async Task<IActionResult> GuardarRutina(int rutinaId)
        {
            var usuarioResult = await _usuarioService.GuardarRutinaActiva(UsuarioId, rutinaId);
            
            if (!usuarioResult.Success)
            {
                return BadRequest(usuarioResult);
            }

            return Ok(usuarioResult);
        }
    }
}
