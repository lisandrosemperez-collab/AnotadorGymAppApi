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
        public EstadoEntrenamiento Estado { get; set; } = EstadoEntrenamiento.EnCurso;

        public ICollection<EjercicioEntrenado> Ejercicios { get; set; }
            = new List<EjercicioEntrenado>();
    }
}

public enum EstadoEntrenamiento
{
    EnCurso,
    Completado,
    Cancelado
}
