using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Entrenamiento.DTOs
{
    public class EjercicioEntrenadoDto
    {
        public int? EjercicioEntrenadoId { get; set; }
        [Required]        
        public int EjercicioId { get; set; }
        public EjercicioSimpleDTO? Ejercicio { get; set; }
        public int Orden { get; set; }
        public string? Notas { get; set; }
        public bool Completado { get; set; } = false;
        public List<SerieEntrenadaDto>? Series { get; set; } = new List<SerieEntrenadaDto>();
    }
}
