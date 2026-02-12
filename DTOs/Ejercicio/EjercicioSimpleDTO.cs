namespace AnotadorGymAppApi.DTOs.Ejercicio
{
    public class EjercicioSimpleDTO
    {                
        public int EjercicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string MusculoPrimarioNombre { get; set; } = string.Empty;
        public string GrupoMuscularNombre { get; set; } = string.Empty;
        public List<string> MusculosSecundariosNombres { get; set; } = new List<string>();
        public EjercicioSimpleDTO() { }
    }
}
