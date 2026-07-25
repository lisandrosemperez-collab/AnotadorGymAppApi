using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Entrenamiento.DTOs
{
    public class EntrenamientoDto
    {
        public int? EntrenamientoId { get; set; }        
        [Required]
        public System.DateTime Fecha { get; set; }
        public int? DuracionSegundos { get; set; }
        public string? Notas { get; set; }
        public bool Finalizado { get; set; }
        public List<EjercicioEntrenadoDto>? Ejercicios { get; set; } = new List<EjercicioEntrenadoDto>();
    }
}
