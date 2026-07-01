using AnotadorGymApp.Api.Features.Usuarios.DTO;
using AnotadorGymApp.Api.Features.Usuarios.Results;
using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Features.Usuarios.DTO;

namespace AnotadorGymApp.Api.Features.Usuarios
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObtenerUsuarioInvitado();
        Task <AuthResult> RegistrarUsuario(RegistroRequestDto nuevoUsuarioDto);        
        public Task<AuthResult> LoginUsuario(LoginRequestDto request);
    }
}
