namespace AnotadorGymAppApi.Domain.Entities
{
    public class RutinaSemana
    {
        public int NumeroSemana { get; set; }
        public int RutinaSemanaId { get; set; }
        public int RutinaId { get; set; }
        public Rutina? Rutina { get; set; }
        public ICollection<RutinaDia> Dias { get; set; } = new List<RutinaDia>();
        public string NombreSemana { get; set; } = string.Empty;
        public RutinaSemana() { }
    }
}
