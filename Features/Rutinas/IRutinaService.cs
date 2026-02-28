using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Features.Rutinas.Results;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public interface IRutinaService
    {
        public Task<RutinaListResult> GetAllRutinas();
        public Task<RutinaDto> GetRutina(string nombre);
    }
}
