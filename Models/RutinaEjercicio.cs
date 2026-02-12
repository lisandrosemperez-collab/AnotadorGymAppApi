using AnotadorGymAppApi.Models;

namespace AnotadorGymAppApi.Entities
{
    public class RutinaEjercicio
    {
        public int RutinaEjercicioId { get; set; }        
        public int RutinaDiaId { get; set; }
        public int EjercicioId { get; set; }        
        public Ejercicio? Ejercicio { get; set; }
        public RutinaDia? RutinaDia { get; set; }
        public ICollection<RutinaSerie> Series { get; set; } = new List<RutinaSerie>();
        public RutinaEjercicio() { }
    }
}
