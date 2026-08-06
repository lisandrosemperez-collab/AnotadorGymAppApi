using AnotadorGymAppApi.Domain.Entities.Usuario;

namespace AnotadorGymApp.Api.Domain.Entities.Entrenamiento
{
    public class Entrenamiento
    {
        public int EntrenamientoId { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public DateTime Fecha { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public int? DuracionSegundos { get; set; }

        public string? Notas { get; set; }
        public bool Completado { get; set; } = false;

        public ICollection<EjercicioEntrenado> Ejercicios { get; set; }
            = new List<EjercicioEntrenado>();
    }
}
