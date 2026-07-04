using AnotadorGymAppApi.Domain.Entities.Ejercicio;

namespace AnotadorGymApp.Api.Domain.Entities.Entrenamiento
{
    public class EjercicioEntrenado
    {
        public int EjercicioEntrenadoId { get; set; }

        public int EntrenamientoId { get; set; }
        public Entrenamiento Entrenamiento { get; set; }

        public int EjercicioId { get; set; }
        public Ejercicio Ejercicio { get; set; }

        public int Orden { get; set; }

        public string? Notas { get; set; }

        public ICollection<SerieEntrenada> Series { get; set; }
            = new List<SerieEntrenada>();
    }
}
