namespace AnotadorGymAppApi.DTOs.ImportResult
{
    public class ImportErrorDTO
    {
        public int Indice { get; set; }
        public string? NombreEjercicio { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public ImportErrorDTO() { }

    }
}
