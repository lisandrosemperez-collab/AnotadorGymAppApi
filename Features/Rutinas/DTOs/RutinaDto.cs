using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Rutinas.DTOs
{
    public class RutinaDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string FrecuenciaPorGrupo { get; set; }
        public string Dificultad { get; set; }
        public string TiempoPorSesion { get; set; }
        public string ImageSource { get; set; }
        public List<RutinaSemanaDto> Semanas { get; set; }
    }
    public class RutinaSemanaDto
    {                
        public List<RutinaDiaDto> Dias { get; set; }
        public int NumeroSemana { get; set; }
    }
    public class RutinaDiaDto
    {                
        public List<RutinaEjercicioDto> Ejercicios { get; set; }
        public int NumeroDia { get; set; }
    }
    public class RutinaEjercicioDto
    {        
        public EjercicioSimpleDTO Ejercicio { get; set; }
        public List<RutinaSerieDto> Series { get; set; }
        public int NumeroEjercicio { get; set; }
    }

    public class RutinaSerieDto
    {
        public int Repeticiones { get; set; }
        public int Porcentaje1RM { get; set; }
        public string Descanso { get; set; }    
        public TipoSerie Tipo { get; set; }
        public int NumeroSerie { get; set; }
    }
}
