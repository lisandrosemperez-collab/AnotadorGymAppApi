using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    [Route("api/ejercicios")]
    [ApiController]
    public class EjercicioController : ControllerBase
    {
        private readonly ILogger<EjercicioController> _logger;
        private readonly IEjercicioImport _ejercicioImportService;
        private readonly IEjercicioService _ejercicioService;
        public EjercicioController(ILogger<EjercicioController> logger, IEjercicioService ejercicioService, IEjercicioImport ejercicioImport)
        {
            _ejercicioService = ejercicioService; 
            _ejercicioImportService = ejercicioImport;         
            _logger = logger;
        }

        /// <summary>
        /// Importa ejercicios desde un archivo JSON y los guarda en la base de datos.
        /// </summary>
        /// <remarks>
        /// **⚠️ Requiere autenticación mediante token JWT con rol Admin.**
        /// 
        /// Para obtener un token de acceso, contacta al administrador en:
        /// **[lisandrosemperez@gmail.com](mailto:lisandrosemperez@gmail.com)**
        /// 
        /// Incluye el token en el encabezado `Authorization: Bearer {token}`.
        /// 
        /// Este endpoint **procesa y persiste** los ejercicios contenidos en el archivo.
        ///
        /// **Requisitos del archivo:**
        /// - Extensión **.json**
        /// - Tamaño máximo: **10 MB**
        /// - Nombre del campo del formulario: **archivo**
        /// - Contenido: array JSON de objetos con la misma estructura que `EjercicioDTO`.
        ///
        /// **Validaciones adicionales:**
        /// - No se permiten ejercicios duplicados (por nombre único).
        /// - Se devuelve un resumen con los resultados de la importación.
        ///        
        ///
        /// **Ejemplo de petición (multipart/form-data):**
        /// ```
        /// POST /api/importacion/ejercicios
        /// Content-Type: multipart/form-data
        /// Authorization: Bearer {token}
        /// 
        /// --boundary
        /// Content-Disposition: form-data; name="archivo"; filename="ejercicios.json"
        /// Content-Type: application/json
        /// 
        /// [ ... contenido JSON ... ]
        /// --boundary--
        /// ```
        ///
        /// **Respuesta exitosa (200 OK):**
        /// ```json
        /// {
        ///   "ejerciciosImportados": 2,
        ///   "ejerciciosOmitidos": 0,
        ///   "errores": [],
        ///   "mensaje": "Importación completada exitosamente",
        ///   "duracion": "00:00:00.452"
        /// }
        /// ```
        ///
        /// **Respuesta con errores (200 OK con errores):**
        /// ```json
        /// {
        ///   "ejerciciosImportados": 1,
        ///   "ejerciciosOmitidos": 1,
        ///   "errores": [
        ///     {
        ///       "nombreEjercicio": "Press Banca",
        ///       "mensaje": "El ejercicio ya existe en la base de datos"
        ///     }
        ///   ],
        ///   "mensaje": "Importación completada con errores",
        ///   "duracion": "00:00:00.231"
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) con los ejercicios a importar.</param>
        /// <returns>Resultado de la importación con contadores y posibles errores.</returns>
        /// <response code="200">Importación procesada. Revisar el objeto de resultado para ver detalles.</response>
        /// <response code="400">Archivo inválido, vacío, extensión incorrecta o datos mal formados.</response>
        /// <response code="401">No autenticado. Token JWT faltante o inválido.</response>
        /// <response code="403">Autenticado pero sin rol de Admin.</response>
        /// <response code="413">Archivo demasiado grande (supera 10MB).</response>
        /// <response code="415">Tipo de contenido no soportado (debe ser multipart/form-data).</response>
        /// <response code="500">Error interno del servidor.</response>   
        [HttpPost("importar")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]        
        [ProducesResponseType(typeof(ImportResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ImportResultDTO>> ImportarEjerciciosDesdeArchivo([Required] IFormFile archivo)
        {
            _logger.LogInformation("Solicitud de importación recibida. Archivo: {FileName}, Tamaño: {Size}", archivo?.FileName, archivo?.Length);           
                                                                                   
            var importResult = await _ejercicioImportService.ImportarEjerciciosDesdeArchivoAsync(archivo);
            if (importResult.FalloCritico == true) return BadRequest(new ProblemDetails
            {
                Title = "Error crítico en la importación",
                Detail = importResult.Errores.FirstOrDefault()?.Mensaje ?? "Ocurrió un error crítico durante la importación",
                Status = StatusCodes.Status400BadRequest
            });
                
            return Ok(importResult);
                       
        }

        /// <summary>
        /// Verifica que el formato de un archivo JSON sea válido para importación de ejercicios.
        /// </summary>
        /// <remarks>
        /// Este endpoint **no guarda** los datos, solo valida que el archivo tenga la estructura esperada para una importación de ejercicios.
        /// 
        /// **Requisitos del archivo:**
        /// - Extensión **.json**
        /// - Tamaño máximo: **10 MB**
        /// - Nombre del campo del formulario: **archivo**
        /// - Contenido: array JSON de objetos con la misma estructura que `EjercicioDTO`.
        ///
        /// La validación comprueba que el JSON pueda deserializarse correctamente en una lista de `EjercicioDTO`.
        /// No se guarda ningún dato en la base de datos, no se verifica la existencia de duplicados,
        /// solo se valida la estructura del JSON.
        /// 
        /// **Validaciones básicas:**
        /// - El nombre del ejercicio no puede estar vacío
        /// - El musculoPrimarioId debe ser un número válido (opcional según tu lógica)
        /// 
        /// **Ejemplo de archivo válido:**
        /// ```json
        /// [
        ///   {
        ///     "nombre": "Press Banca",
        ///     "musculoPrimarioId": 1,
        ///     "musculoSecundarioId": 3,
        ///     "notas": "Ejercicio compuesto para pecho"
        ///   },
        ///   {
        ///     "nombre": "Sentadilla",
        ///     "musculoPrimarioId": 5,
        ///     "notas": "Ejercicio fundamental para piernas"
        ///   },
        ///   {
        ///     "nombre": "Dominadas",
        ///     "musculoPrimarioId": 4,
        ///     "musculoSecundarioId": 2,
        ///     "notas": "Ejercicio de espalda"
        ///   }
        /// ]
        /// ```
        /// 
        /// **Respuesta exitosa (200 OK):**
        /// ```json
        /// {
        ///   "esValido": true,
        ///   "cantidadEjercicios": 3,
        ///   "mensaje": "Formato válido. 3 ejercicio(s) detectado(s)."
        /// }
        /// ```
        /// 
        /// **Respuesta con formato inválido (200 OK):**
        /// ```json
        /// {
        ///   "esValido": false,
        ///   "cantidadEjercicios": 0,
        ///   "mensaje": "Formato inválido. No se pudo deserializar el contenido en una lista de ejercicios."
        /// }
        /// ```
        /// 
        /// **Respuesta con error de validación de datos (200 OK):**
        /// ```json
        /// {
        ///   "esValido": false,
        ///   "cantidadEjercicios": 2,
        ///   "mensaje": "Formato inválido. Uno o más ejercicios no tienen nombre."
        /// }
        /// ```
        /// 
        /// **Respuesta con error de archivo vacío (200 OK):**
        /// ```json
        /// {
        ///   "esValido": false,
        ///   "cantidadEjercicios": 0,
        ///   "mensaje": "El archivo está vacío o no se proporcionó."
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) con los ejercicios a validar.</param>
        /// <returns>Resultado de la validación con detalles sobre el formato.</returns>
        /// <response code="200">Validación completada. El objeto `FormatCheckResult` indica si el formato es válido y la cantidad de ejercicios.</response>
        /// <response code="400">Si la extensión del archivo no es .json.</response>
        /// <response code="415">Si el contenido no es multipart/form-data.</response>
        [HttpPost("importar/validar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [ProducesResponseType(typeof(FormatCheckResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<ActionResult<FormatCheckResult>> VerificarFormatoArchivo([Required] IFormFile archivo)
        {
            _logger.LogInformation("Verificando formato de archivo: {Nombre}, {Tamaño} bytes", archivo?.FileName, archivo?.Length);
            if (archivo == null || archivo.Length == 0)
            {
                _logger.LogInformation("Archivo inválido o vacío");
                return Ok(new FormatCheckResult { EsValido = false, Mensaje = "Archivo vacío" });
            }
            if (!archivo.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Formato de archivo inválido",
                    Detail = "Solo se permiten archivos con extensión .json",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var jsonContent = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var ejercicios = JsonSerializer.Deserialize<List<EjercicioDTO>>(jsonContent, options);

                if (ejercicios != null)
                {
                    return Ok(new FormatCheckResult
                    {
                        EsValido = true,
                        CantidadEjercicios = ejercicios.Count,
                        CantidadRutinas = 0,
                        Mensaje = $"Formato válido. {ejercicios.Count} ejercicios detectados."
                    });
                }
                else
                {
                    return Ok(new FormatCheckResult
                    {
                        EsValido = false,
                        CantidadEjercicios = 0,
                        CantidadRutinas = 0,
                        Mensaje = "Formato inválido. El archivo no contiene una lista válida del tipo esperado."
                    });
                }

            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error de deserialización JSON en archivo de ejercicios");
                return Ok(new FormatCheckResult
                {
                    EsValido = false,
                    CantidadRutinas = 0,
                    CantidadEjercicios = 0,
                    Mensaje = $"Formato inválido. Error en la estructura JSON: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al validar archivo de ejercicios");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error inesperado al procesar la validación",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtiene todos los ejercicios registrados.
        /// </summary>
        /// <remarks>
        /// **⚠️ Requiere autenticación mediante token JWT.**
        /// 
        /// Para obtener un token de acceso, contacta al administrador en:
        /// **[lisandrosemperez@gmail.com](mailto:lisandrosemperez@gmail.com)**
        /// 
        /// Incluye el token en el encabezado `Authorization: Bearer {token}`.
        /// 
        /// Este endpoint devuelve la colección completa de ejercicios.
        /// Requiere rol <c>Admin</c>.
        /// </remarks>
        /// <returns>Lista completa de ejercicios.</returns>
        /// <response code="200">Ejercicios obtenidos correctamente.</response>
        /// <response code="401">Usuario no autenticado.</response>
        /// <response code="403">Usuario autenticado sin permisos suficientes.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EjercicioDTO>>> GetEjercicios()
        {
            var ejercicios = await _ejercicioService.GetAllEjercicios();
            return Ok(ejercicios);
        }

        /// <summary>
        /// Obtiene ejercicios con filtros opcionales, paginación y orden.
        /// </summary>
        /// <remarks>
        /// Permite filtrar por nombre, paginar resultados y ordenar dinámicamente.
        /// Todos los parámetros se envían por query string.
        /// </remarks>
        /// <response code="200">Listado obtenido correctamente.</response>
        /// <response code="400">Parámetros inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<PagedResult<EjercicioDTO>>> GetEjercicios([FromQuery] PaginationParams pagination)
        {
            var result = await _ejercicioService.GetEjercicios(pagination);

            Response.Headers["X-Total-Count"] = result.totalCount.ToString();
            Response.Headers["X-Page-Number"] = pagination.Page.ToString();
            Response.Headers["X-Page-Size"] = pagination.PageSize.ToString();

            return Ok(new PagedResult<EjercicioDTO>
            {
                Items = result.items,
                TotalCount = result.totalCount,
                PageNumber = pagination.Page,
                PageSize = pagination.PageSize
            });
        }        


    }
}
