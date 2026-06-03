using AnotadorGymAppApi.Domain.Entities.Ejercicio;

namespace AnotadorGymAppApi.Domain.Entities.Rutina
{
    public class RutinaEjercicio
    {
        public int NumeroEjercicio { get; set; }
        public int RutinaEjercicioId { get; set; }        
        public int RutinaDiaId { get; set; }
        public int EjercicioId { get; set; }        
        public Ejercicio.Ejercicio? Ejercicio { get; set; }
        public RutinaDia? RutinaDia { get; set; }
        public ICollection<RutinaSerie> Series { get; set; } = new List<RutinaSerie>();
        public RutinaEjercicio() { }
    }
}
