using AnotadorGymApp.Api.Domain.Entities.Entrenamiento;
using AnotadorGymAppApi.Domain.Entities.Rutina;

namespace AnotadorGymAppApi.Domain.Entities.Usuario
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string UserName {  get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public string Rol {  get; set; }
        public ICollection<Entrenamiento> Entrenamientos { get; set; } = new List<Entrenamiento>();
        public AnotadorGymAppApi.Domain.Entities.Rutina.Rutina? RutinaActiva { get; set; }
        public int? RutinaActivaId { get; set; }
    }
}
