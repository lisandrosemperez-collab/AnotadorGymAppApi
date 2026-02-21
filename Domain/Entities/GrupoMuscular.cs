using System.ComponentModel.DataAnnotations;

namespace AnotadorGymAppApi.Domain.Entities
{
    public class GrupoMuscular
    {
        public ICollection<Ejercicio> Ejercicios { get; set; } = new List<Ejercicio>();
        [Key]
        public int GrupoMuscularId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public GrupoMuscular() { }
    }
}
