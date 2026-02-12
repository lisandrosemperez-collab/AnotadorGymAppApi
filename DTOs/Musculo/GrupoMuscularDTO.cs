using AnotadorGymAppApi.Models;

namespace AnotadorGymAppApi.DTOs.Musculo
{
    public class GrupoMuscularDTO
    {        
        public int GrupoMuscularId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public GrupoMuscularDTO() { }

    }
}
