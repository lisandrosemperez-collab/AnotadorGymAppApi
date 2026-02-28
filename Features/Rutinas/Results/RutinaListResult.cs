using AnotadorGymAppApi.Features.Rutinas.DTOs;

namespace AnotadorGymAppApi.Features.Rutinas.Results
{
    public class RutinaListResult
    {
        public List<RutinaDto> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
