namespace AnotadorGymAppApi.Features.Common.Results
{
    public class FormatCheckResult
    {
        public bool EsValido { get; set; }
        public int CantidadEjercicios { get; set; }
        public int CantidadRutinas { get; set; }
        public string Mensaje { get; set; }
    }
}
