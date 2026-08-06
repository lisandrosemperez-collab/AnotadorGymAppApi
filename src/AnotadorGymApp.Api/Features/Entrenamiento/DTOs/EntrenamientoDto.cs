using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Entrenamiento.DTOs
{
    public class EntrenamientoDto
    {
        public int? EntrenamientoId { get; set; }        
        [Required]
        public DateTime Fecha { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public int? DuracionSegundos { get; set; }
        public string? Notas { get; set; }
        public bool Completado { get; set; }
        public List<EjercicioEntrenadoDto>? Ejercicios { get; set; } = new List<EjercicioEntrenadoDto>();
    }
}
