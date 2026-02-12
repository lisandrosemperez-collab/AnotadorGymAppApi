using AnotadorGymAppApi.DTOs.Ejercicio;
using AnotadorGymAppApi.DTOs.ImportResult;
using AnotadorGymAppApi.Services.Implementations;
using AnotadorGymAppApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace AnotadorGymAppApi.Controllers
{
    [Route("api/imports")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class ImportController : ControllerBase
    {
        private readonly ILogger<ImportController> _logger;      
        private readonly IImportService _importService;
        public ImportController(ILogger<ImportController> logger,IImportService importService)
        {
            _logger = logger;            
            _importService = importService;
        }

        /// <summary>
        /// Importa ejercicios desde un archivo JSON y los guarda en la base de datos.
        /// </summary>
        /// <remarks>
        /// **⚠️ Requiere autenticación mediante token JWT.**
        /// 
        /// Para obtener un token de acceso, contacta al administrador en:
        /// **[lisandrosemperez@gmail.com](mailto:lisandrosemperez@gmail.com)**
        /// 
        /// Incluye el token en el encabezado `Authorization: Bearer {token}`.
        /// 
        /// Este endpoint **procesa y persiste** los ejercicios contenidos en el archivo.
        ///
        /// **Requisitos del archivo:**
        /// - Extensión `.json`
        /// - Tamaño máximo: **10 MB**
        /// - Contenido: array JSON de objetos con la misma estructura que `EjercicioDTO`.
        ///
        /// **Validaciones adicionales:**
        /// - No se permiten ejercicios duplicados (por nombre único).
        /// - Se devuelve un resumen con los resultados de la importación.
        ///
        /// Ejemplo de petición (multipart/form-data):
        /// ```
        /// Content-Disposition: form-data; name="archivo"; filename="ejercicios.json"
        /// Content-Type: application/json
        /// ```
        ///
        /// **Respuesta exitosa (201 Created):**
        /// ```json
        /// {
        ///   "ejerciciosImportados": 2,
        ///   "ejerciciosOmitidos": 0,
        ///   "errores": [],
        ///   "mensaje": "Importación completada exitosamente"
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) con los ejercicios a importar.</param>
        /// <returns>Resultado de la importación con contadores y posibles errores.</returns>
        /// <response code="201">Importación exitosa. Devuelve el resumen.</response>
        /// <response code="400">Archivo inválido, vacío, extensión incorrecta o datos erróneos.</response>
        /// <response code="413">Archivo demasiado grande (supera 10MB).</response>
        /// <response code="415">Tipo de contenido no soportado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]        
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportResultDTO), StatusCodes.Status201Created)]        
        public async Task<ActionResult<ImportResultDTO>> ImportarEjerciciosDesdeArchivo([Required] IFormFile archivo)
        {
            _logger.LogInformation("Recibida solicitud de importación desde archivo: {Nombre}",
                archivo?.FileName);

            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Archivo inválido",
                    Detail = "No se proporcionó archivo o está vacío",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                if (Path.GetExtension(archivo.FileName).ToLower() != ".json")
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de archivo inválido",
                        Detail = "Solo se permiten archivos con extensión .json",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (archivo.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Archivo muy grande",
                        Detail = "El archivo no debe superar los 10MB",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var importResult = await _importService.ImportarEjerciciosDesdeArchivoAsync(archivo);
                return Created("",importResult);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error en los datos del archivo");
                return BadRequest(new ProblemDetails
                {
                    Title = "Datos inválidos",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al importar ejercicios");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error inesperado al procesar la importación",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }



        /// <summary>
        /// Verifica que el formato de un archivo JSON sea válido para importación.
        /// </summary>
        /// <remarks>
        /// 
        /// **⚠️ Requiere autenticación mediante token JWT.**
        /// 
        /// Para obtener un token de acceso, contacta al administrador en:
        /// **[lisandrosemperez@gmail.com](mailto:lisandrosemperez@gmail.com)**
        /// 
        /// Incluye el token en el encabezado `Authorization: Bearer {token}`.
        /// 
        /// Este endpoint **no guarda** los datos, solo valida que el archivo tenga la estructura esperada:
        ///
        /// - El archivo debe ser un array JSON de objetos con las propiedades de un ejercicio.
        /// - Se verifica que el JSON sea deserializable como `List&lt;EjercicioDTO&gt;`.
        /// - No se requiere autenticación.
        ///
        /// Ejemplo de archivo válido:
        ///
        /// ```json
        /// [
        ///   {
        ///     "nombre": "Press Banca",
        ///     "musculoPrimarioId": 1,
        ///     "notas": "Ejercicio compuesto"
        ///   },
        ///   {
        ///     "nombre": "Sentadilla",
        ///     "musculoPrimarioId": 2
        ///   }
        /// ]
        /// ```
        ///
        /// **Respuesta exitosa (200 OK):**
        /// ```json
        /// {
        ///   "esValido": true,
        ///   "cantidadEjercicios": 2,
        ///   "mensaje": "Formato válido. 2 ejercicios detectados"
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) que contiene la lista de ejercicios.</param>
        /// <returns>Resultado de la validación con cantidad de ejercicios y mensaje.</returns>
        /// <response code="200">Validación completada (el archivo puede ser válido o inválido).</response>
        /// <response code="400">Si no se envía archivo o está vacío (aunque en este endpoint se maneja como 200 con error).</response>
        /// <response code="415">Si el contenido no es multipart/form-data.</response>
        [HttpPost("validate")]        
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FormatCheckResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<FormatCheckResult>> VerificarFormatoArchivo([Required] IFormFile archivo)
        {
            _logger.LogInformation("Verificando formato de archivo: {Nombre}, {Tamaño} bytes",archivo?.FileName, archivo?.Length);
            if (archivo == null || archivo.Length == 0)
            {
                return Ok(new FormatCheckResult { EsValido = false, Mensaje = "Archivo vacío" });
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                using var reader = new StreamReader(stream,Encoding.UTF8);
                var jsonContent = await reader.ReadToEndAsync();

                var ejercicios = JsonSerializer.Deserialize<List<EjercicioDTO>>(jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                return Ok(new FormatCheckResult
                {
                    EsValido = ejercicios != null,
                    CantidadEjercicios = ejercicios?.Count ?? 0,
                    Mensaje = ejercicios != null ?
                        $"Formato válido. {ejercicios.Count} ejercicios detectados" :
                        "Formato inválido"
                });
            }
            catch (Exception ex)
            {
                return Ok(new FormatCheckResult
                {
                    EsValido = false,
                    Mensaje = $"Error: {ex.Message}"
                });
            }
        }
        
    }
}
