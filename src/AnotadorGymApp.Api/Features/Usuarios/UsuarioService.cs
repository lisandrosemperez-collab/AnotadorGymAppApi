using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Features.Usuarios.DTO;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymAppApi.Features.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _appDbContext;
        public UsuarioService(AppDbContext appDbContext) { _appDbContext = appDbContext; }

        public async Task<Usuario?> ObtenerUsuarioInvitado()
        {
            var usuarioInvitado = await _appDbContext.Usuarios.
                FirstOrDefaultAsync(u => u.Rol == "Invitado");
            
            return usuarioInvitado;
        }

        public async Task<Usuario?> ValidarUsuario(UsuarioDto request)
        {
            var usuario = await _appDbContext.Usuarios.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            
            if (usuario == null)
                return usuario;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);

            return isPasswordValid ? usuario : null;
        }
    }
}
