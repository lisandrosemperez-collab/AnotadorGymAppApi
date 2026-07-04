namespace AnotadorGymApp.Api.Domain.Entities.Entrenamiento
{
    public class SerieEntrenada
    {
        public int SerieEntrenadaId { get; set; }

        public int EjercicioEntrenadoId { get; set; }
        public EjercicioEntrenado EjercicioEntrenado { get; set; }

        public int NumeroSerie { get; set; }

        public decimal Peso { get; set; }

        public int Repeticiones { get; set; }

        public bool Completada { get; set; }

        public bool FuePR { get; set; }

        public int? RPE { get; set; }

        public int? DescansoSegundos { get; set; }
    }
}
