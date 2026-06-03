namespace AnotadorGymAppApi.Features.Common.Validation
{
    public interface IJsonFileValidator
    {
        Task<(bool esValido, T? Data, string? Error)> ValidateJsonFileAsync<T>(IFormFile archivo);
    }
}
