using AnotadorGymAppApi.Models;

namespace AnotadorGymAppApi.DTOs.Musculo
{
    public class MusculoDTO
    {        
        public int MusculoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public MusculoDTO() { }
    }
}
