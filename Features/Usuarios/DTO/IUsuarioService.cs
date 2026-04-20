using AnotadorGymAppApi.Domain.Entities.Usuario;

namespace AnotadorGymAppApi.Features.Usuarios.DTO
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObtenerUsuarioInvitado();

        /// <summary>
        /// Verifica si las credenciales del usuario son válidas comparando con la base de datos de Neon.
        /// </summary>
        /// <param name="request">DTO que contiene UserName y Password (en texto plano).</param>
        /// <returns>La entidad Usuario completa si es válido; de lo contrario, null.</returns>  
        public Task<Usuario?> ValidarUsuario(UsuarioDto usuario);
    }
}
