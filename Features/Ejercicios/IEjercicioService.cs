using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Ejercicios.Results;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public interface IEjercicioService
    {
        public Task<(List<EjercicioDTO?>, bool)> GetAllEjercicios();                
        public Task<(List<EjercicioDTO?>, int,bool)> GetEjercicios(PaginationParams pagination);        
        Task<bool> EliminarEjercicioAsync(int ejercicioId);
        public Task<(EjercicioDTO?,bool)> GetPorId(int id);        
        public Task<EjercicioSimpleDTO?> ActualizarEjercicioAsync(int id, EjercicioSimpleDTO dto);
        Task<ActualizarResult> ActualizarEjerciciosAsync(List<EjercicioSimpleDTO> dtos);

    }
}
