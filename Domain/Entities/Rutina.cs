namespace AnotadorGymAppApi.Domain.Entities
{
    public class Rutina
    {
        public int RutinaId { get; set; }        
        public ICollection<RutinaSemana> Semanas { get; set; } = new List<RutinaSemana>();
        public string ImageSource { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TiempoPorSesion { get; set; }  = string.Empty;
        public string Dificultad { get; set; }  = string.Empty;
        public string FrecuenciaPorGrupo { get; set; }  = string.Empty;
        public Rutina(){}
    }
}
