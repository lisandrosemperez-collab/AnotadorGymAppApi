using AnotadorGymAppApi.Entities;
using System.ComponentModel.DataAnnotations;

namespace AnotadorGymAppApi.Models
{
    public class Ejercicio
    {
        #region EF
        public ICollection<Musculos> MusculosSecundarios { get; set; } = new List<Musculos>();
        public ICollection<RutinaEjercicio> RutinasEjercicios { get; set; } = new List<RutinaEjercicio>();
        public Musculos? MusculoPrimario { get; set; }
        public int MusculoPrimarioId { get; set; }
        public GrupoMuscular? GrupoMuscular { get; set; }
        public int GrupoMuscularId { get; set; }
        public int EjercicioId { get; set; }
        #endregion        
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Ejercicio() { }
    }
}
