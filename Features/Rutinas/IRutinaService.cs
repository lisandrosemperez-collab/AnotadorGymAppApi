using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public interface IRutinaService
    {
        public Task<List<Rutina>> GetAllRutinas();
        public Task<(List<Rutina> items, int totalCount)> GetRutinas(PaginationParams pagination);
    }
}
