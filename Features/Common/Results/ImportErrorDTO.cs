namespace AnotadorGymAppApi.Features.Common.Results
{
    public class ImportErrorDTO
    {
        public int Indice { get; set; }
        public string? NombreEjercicio { get; set; }
        public string? NombreRutina { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public ImportErrorDTO() { }

    }
}
