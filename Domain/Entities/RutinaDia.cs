namespace AnotadorGymAppApi.Domain.Entities
{
    public class RutinaDia
    {
        public int NumeroDia { get; set; }
        public int RutinaDiaId { get; set; }        
        public int RutinaSemanaId { get; set; }
        public RutinaSemana? RutinaSemana { get; set; }
        public string NombreRutinaDia { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public ICollection<RutinaEjercicio> Ejercicios { get; set; } = new List<RutinaEjercicio>();
        public RutinaDia() { }
    }
}
