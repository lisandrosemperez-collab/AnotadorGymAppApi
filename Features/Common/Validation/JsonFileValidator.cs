using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AnotadorGymAppApi.Features.Common.Validation
{
    public class JsonFileValidator : IJsonFileValidator
    {
        public async Task<(bool esValido, T? Data, string? Error)> ValidateJsonFileAsync<T>(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0) return (false, default, "Archivo vacío");
            
            if (archivo.Length > 10 * 1024 * 1024) return (false, default, "El archivo no debe superar los 10MB");            

            if (!archivo.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return (false, default, "Extensión inválida");

            using var stream = archivo.OpenReadStream();
            using var reader = new StreamReader(stream);
            var contenido = reader.ReadToEnd();

            try
            {                
                var data = JsonSerializer.Deserialize<T>(contenido, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return (data != null, data, null);
            }
            catch (Exception ex)
            {
                return (false, default, $"Error al procesar el archivo: {ex.Message}");                
            }
        }
    }
}
