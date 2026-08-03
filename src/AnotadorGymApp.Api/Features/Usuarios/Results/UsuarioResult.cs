using Microsoft.EntityFrameworkCore.Metadata;

namespace AnotadorGymApp.Api.Features.Usuarios.Results
{
    public class UsuarioResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int? RutinaActivaId { get; set; } = 0;
    }
}
