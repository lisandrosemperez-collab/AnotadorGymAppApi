using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Features.Usuarios.DTO;
using AnotadorGymAppApi.Infrastructure.Context;
using AnotadorGymAppApi.Infrastructure.Security;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AnotadorGymAppApi.Features.Usuarios
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IUsuarioService _usuarioService;
        public AuthController(IJwtProvider jwtProvider,IUsuarioService usuarioService)
        {
            _jwtProvider = jwtProvider;
            _usuarioService = usuarioService;
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioDto request)
        {
            var validar = await _usuarioService.ValidarUsuario(request);

            if (!validar)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            var usuario = new Usuario()
            {
                UserName = request.UserName,
                Email = request.UserName,
                PasswordHash = request.PasswordHash,
                Rol = request.Rol,
            };
            var token = _jwtProvider.GenerarJwtToken(usuario);

            return Ok(token);
        }
    }
}
