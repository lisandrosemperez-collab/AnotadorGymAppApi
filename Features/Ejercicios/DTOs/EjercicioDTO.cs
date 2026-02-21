namespace AnotadorGymAppApi.Features.Ejercicios.DTOs
{
    public class EjercicioDTO
    {       
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public MusculoDTO? MusculoPrimario { get; set; } = new MusculoDTO();
        public List<MusculoDTO>? MusculosSecundarios { get; set; } = new List<MusculoDTO>();
        public GrupoMuscularDTO GrupoMuscular { get; set; }
        public EjercicioDTO() { }
    }
}
