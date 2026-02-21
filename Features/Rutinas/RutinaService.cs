using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public class RutinaService : IRutinaService
    {
        public Task<List<Rutina>> GetAllRutinas()
        {
            throw new NotImplementedException();
        }

        public Task<(List<Rutina> items, int totalCount)> GetRutinas(PaginationParams pagination)
        {
            throw new NotImplementedException();
        }
    }
}
