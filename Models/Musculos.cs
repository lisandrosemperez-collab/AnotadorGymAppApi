namespace AnotadorGymAppApi.Models
{
    public class Musculos
    {
        public ICollection<Ejercicio> EjerciciosSecundarios { get; set; } = new List<Ejercicio>();
        public ICollection<Ejercicio> EjerciciosPrimarios { get; set; } = new List<Ejercicio>();        
        public int MusculoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public Musculos() { }
    }
}
