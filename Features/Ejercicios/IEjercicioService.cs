using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public interface IEjercicioService
    {
        public Task<List<EjercicioDTO>> GetAllEjercicios();                
        public Task<(List<EjercicioDTO> items, int totalCount)> GetEjercicios(PaginationParams pagination);        
        Task<bool> EliminarEjercicioAsync(int ejercicioId);
        public Task<EjercicioDTO> ActualizarEjercicioAsync(int id, EjercicioSimpleDTO dto);
        public Task<EjercicioDTO> GetPorId(int id);
    }
}
