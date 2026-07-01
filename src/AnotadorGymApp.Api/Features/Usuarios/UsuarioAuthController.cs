using AnotadorGymApp.Api.Features.Usuarios;
using AnotadorGymApp.Api.Features.Usuarios.DTO;
using AnotadorGymApp.Api.Features.Usuarios.Results;
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
    public class UsuarioAuthController : ControllerBase
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IUsuarioService _usuarioService;
        public UsuarioAuthController(IJwtProvider jwtProvider, IUsuarioService usuarioService)
        {
            _jwtProvider = jwtProvider;
            _usuarioService = usuarioService;
        }
        /// <summary>
        /// Autentica a un usuario y devuelve un token JWT.
        /// </summary>
        /// <remarks>
        /// Envía el UserName y la Password en el cuerpo de la solicitud. 
        /// Si las credenciales son válidas, recibirás un token con una validez de 1 hora 
        /// que incluye los permisos (Roles) del usuario para acceder a rutas protegidas.                
        /// </remarks>
        /// <returns>Un token JWT firmado si la operación es exitosa.</returns>
        /// <response code="200">Login exitoso.</response>
        /// <response code="401">Usuario no encontrado o contraseña incorrecta.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var resultado = await _usuarioService.LoginUsuario(request);

            if (!resultado.Success)
            {
                return resultado.Error switch
                {
                    AuthError.UsuarioNoExiste => Unauthorized(resultado),
                    AuthError.ContraseñaIncorrecta => Unauthorized(resultado),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, resultado)
                };
            }

            return Ok(resultado);            
        }

        /// <summary>
        /// Solicita autorización como Invitado sin necesidad de credenciales.
        /// Devuelve un token JWT con el rol "Invitado".
        /// </summary>
        /// <remarks>
        /// Este endpoint genera un token JWT que permite acceder a endpoints protegidos
        /// con permisos limitados (solo lectura).
        ///
        /// 🔐 **Cómo usar el token en Swagger:**
        ///
        /// 1. Ejecutar este endpoint (`/login/invitado`)
        /// 2. Copiar el valor de `tokenString` de la respuesta
        /// 3. Hacer clic en el botón **Authorize** (arriba a la derecha)
        /// 4. En el campo mostrado, ingresar el token en el siguiente formato:
        ///
        ///    ```
        ///    Bearer {token}
        ///    ```
        ///
        ///    Ejemplo:
        ///    ```
        ///    Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        ///    ```
        ///
        /// 5. Presionar **Authorize** y luego **Close**
        ///
        /// A partir de ese momento, todas las solicitudes incluirán automáticamente
        /// el token JWT en el header `Authorization`.
        ///
        /// ⏳ El token tiene una validez de 1 hora.
        /// 👤 Incluye el rol "Invitado", por lo que solo permite acceder a endpoints habilitados para dicho rol.
        /// </remarks>
        /// <returns>Un token JWT válido para autenticación como Invitado.</returns>
        /// <response code="200">Token generado correctamente.</response>
        /// <response code="401">No se pudo obtener el usuario Invitado.</response>
        [HttpPost("login/invitado")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginInvitado()
        {
            var usuario = await _usuarioService.ObtenerUsuarioInvitado();
            if (usuario == null)
            {
                return Unauthorized(new { message = "No se pudo obtener Invitado de Base de Datos" });
            }

            var token = _jwtProvider.GenerarJwtToken(usuario);                        

            return Ok(new { tokenString = token });
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="request">Datos del usuario a registrar.</param>
        /// <returns>Resultado de la operación de registro.</returns>
        /// <response code="201">Usuario registrado correctamente.</response>
        /// <response code="409">El usuario ya existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status500InternalServerError)]

        [HttpPost("login/registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequestDto request)
        {
            // Guardar el usuario en la base de datos
            var resultado = await _usuarioService.RegistrarUsuario(request);

            if (!resultado.Success)
            {
                return resultado.Error switch
                {
                    AuthError.UsuarioYaExiste => Conflict(resultado),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, resultado)
                };
            }

            return CreatedAtAction(
                nameof(Login),
                new { userName = resultado.UserName },
                resultado);
        }
    }
}
