namespace AnotadorGymAppApi.Domain.Entities
{
    public class RutinaSerie
    {
        public int NumeroSerie { get; set; }
        public int RutinaSerieId { get; set; }
        public RutinaEjercicio? RutinaEjercicio { get; set; }
        public int RutinaEjercicioId { get; set; }
        public TimeSpan? Descanso { get; set; } = default;
        public int? Repeticiones { get; set; }
        public int? Porcentaje1RM { get; set; }
        public TipoSerie Tipo { get; set; } = TipoSerie.Normal;
        public RutinaSerie() { }
    
    }
    public enum TipoSerie
    {
        Normal,
        DropSet,
        Cluster,
        Myo_Reps,
        Negativas,
        Max_Rm
    }
}
