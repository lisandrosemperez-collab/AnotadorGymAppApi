using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Entrenamiento.DTOs
{
    public class EjercicioEntrenadoDto
    {
        public int? EjercicioEntrenadoId { get; set; }
        [Required]
        public int EjercicioId { get; set; }
        public int Orden { get; set; }
        public string? Notas { get; set; }
        public List<SerieEntrenadaDto>? Series { get; set; } = new List<SerieEntrenadaDto>();
    }
}
