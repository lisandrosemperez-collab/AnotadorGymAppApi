using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public interface IEjercicioService
    {
        public Task<List<EjercicioDTO>> GetAllEjercicios();                
        public Task<(List<EjercicioDTO> items, int totalCount)> GetEjercicios(PaginationParams pagination);        
    }
}
