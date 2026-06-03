namespace AnotadorGymAppApi.Features.Ejercicios.DTOs
{
    public class EjercicioSimpleDTO
    {                        
        public string Nombre { get; set; } = string.Empty;
        public int EjercicioId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? UrlVideo { get; set; }
        public EjercicioSimpleDTO() { }
    }
}
