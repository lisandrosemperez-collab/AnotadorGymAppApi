using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Rutinas.DTOs;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public interface IRutinaService
    {
        public Task<(List<RutinaDto> items, int totalCount)> GetAllRutinas();
        public Task<RutinaDto> GetRutina(string nombre);
    }
}
