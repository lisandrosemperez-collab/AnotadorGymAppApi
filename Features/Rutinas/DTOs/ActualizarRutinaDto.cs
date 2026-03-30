namespace AnotadorGymAppApi.Features.Rutinas.DTOs
{
    public class ActualizarRutinaDto
    {
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Dificultad { get; set; }
        public string? TiempoPorSesion { get; set; }
    }
}
