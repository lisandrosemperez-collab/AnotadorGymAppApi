using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Features.Usuarios.DTO;

namespace AnotadorGymAppApi.Infrastructure.Security
{
    public interface IJwtProvider
    {
        string GenerarJwtToken(Usuario usuario);
    }
}
