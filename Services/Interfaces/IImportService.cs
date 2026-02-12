using AnotadorGymAppApi.DTOs.Ejercicio;
using AnotadorGymAppApi.DTOs.ImportResult;

namespace AnotadorGymAppApi.Services.Interfaces
{
    public interface IImportService
    {
        Task<ImportResultDTO> ImportarEjerciciosDesdeJsonAsync(List<EjercicioDTO> ejerciciosImport);
        Task<ImportResultDTO> ImportarEjerciciosDesdeArchivoAsync(IFormFile archivo);
    }
}
