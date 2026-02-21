using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace AnotadorGymAppApi.Features.Rutinas
{
    [Route("api/rutinas")]
    [ApiController]
    public class RutinaController : ControllerBase
    {
        private readonly ILogger<RutinaController> _logger;        
        private readonly IRutinaImport _rutinaImportService;
        private readonly IRutinaService _rutinaService;
        public RutinaController(ILogger<RutinaController> logger, IRutinaImport rutinaImportService, IRutinaService rutinaService)
        {
            _logger = logger;            
            _rutinaImportService = rutinaImportService;
            _rutinaService = rutinaService;
        }

        /// <summary>
        /// Importa rutinas completas desde un archivo JSON y las guarda en la base de datos.
        /// </summary>
        /// <remarks>
        /// **⚠️ Requiere autenticación mediante token JWT con rol Admin.**
        /// 
        /// Para obtener un token de acceso, contacta al administrador en:
        /// **[lisandrosemperez@gmail.com](mailto:lisandrosemperez@gmail.com)**
        /// 
        /// Incluye el token en el encabezado `Authorization: Bearer {token}`.
        /// 
        /// Este endpoint **procesa y persiste** las rutinas contenidas en el archivo,
        /// incluyendo sus semanas, días, ejercicios y series.
        ///
        /// **Requisitos del archivo:**
        /// - Extensión **.json**
        /// - Tamaño máximo: **10 MB**
        /// - Nombre del campo del formulario: **archivo**
        /// - Contenido: array JSON de objetos con la misma estructura que `RutinaDto`.
        ///
        /// **Validaciones adicionales:**
        /// - Todos los ejercicios referenciados en las rutinas deben existir previamente en la base de datos.
        /// - Si algún ejercicio no existe, la importación se cancela y se reportan los errores.
        /// - Se preserva el orden de semanas, días, ejercicios y series mediante campos numéricos.
        ///
        /// **Ejemplo de archivo JSON válido (estructura simplificada):**
        /// ```json
        /// [
        ///   {
        ///     "nombre": "Rutina Push Pull Legs",
        ///     "descripcion": "Rutina de 3 días",
        ///     "dificultad": "Intermedio",
        ///     "semanas": [
        ///       {
        ///         "numeroSemana": 1,
        ///         "dias": [
        ///           {
        ///             "numeroDia": 1,
        ///             "ejercicios": [
        ///               {
        ///                 "numeroEjercicio": 1,
        ///                 "ejercicio": {
        ///                   "nombre": "Press Banca"
        ///                 },
        ///                 "series": [
        ///                   {
        ///                     "numeroSerie": 1,
        ///                     "repeticiones": 10,
        ///                     "porcentaje1RM": 70,
        ///                     "descanso": "00:02:00",
        ///                     "tipo": 0
        ///                   }
        ///                 ]
        ///               }
        ///             ]
        ///           }
        ///         ]
        ///       }
        ///     ]
        ///   }
        /// ]
        /// ```
        ///
        /// **Ejemplo de petición (multipart/form-data):**
        /// ```
        /// POST /api/importacion/rutinas
        /// Content-Type: multipart/form-data
        /// Authorization: Bearer {token}
        /// 
        /// --boundary
        /// Content-Disposition: form-data; name="archivo"; filename="rutinas.json"
        /// Content-Type: application/json
        /// 
        /// [ ... contenido JSON ... ]
        /// --boundary--
        /// ```
        ///
        /// **Respuesta exitosa (200 OK):**
        /// ```json
        /// {
        ///   "rutinasCreadas": 1,
        ///   "errores": [],
        ///   "mensaje": "Importación completada exitosamente",
        ///   "duracion": "00:00:01.234"
        /// }
        /// ```
        ///
        /// **Respuesta con errores (200 OK con errores):**
        /// ```json
        /// {
        ///   "rutinasCreadas": 0,
        ///   "errores": [
        ///     {
        ///       "nombreEjercicio": "EjercicioInexistente",
        ///       "mensaje": "Ejercicio no encontrado en la base de datos"
        ///     }
        ///   ],
        ///   "mensaje": "No se pudo completar la importación",
        ///   "duracion": "00:00:00.123"
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) con las rutinas a importar.</param>
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
        [RequestSizeLimit(10 * 1024 * 1024)]
        [ProducesResponseType(typeof(ImportResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ImportResultDTO>> ImportarEjerciciosDesdeArchivo([Required] IFormFile archivo)
        {
            _logger.LogInformation("Recibida solicitud de importación desde archivo: {Nombre}", archivo?.FileName);                        
                                                       
            var importResult = await _rutinaImportService.ImportarRutinaDesdeArchivoAsync(archivo);
                
            if (importResult.FalloCritico == true) return BadRequest(new ProblemDetails
            {
                Title = "Error crítico en la importación",
                Detail = importResult.Errores.FirstOrDefault()?.Mensaje ?? "Ocurrió un error crítico durante la importación",
                Status = StatusCodes.Status400BadRequest
            });

            return Ok(importResult);                                    
        }

        /// <summary>
        /// Verifica que el formato de un archivo JSON sea válido para importación de rutinas.
        /// </summary>
        /// <remarks>
        /// Este endpoint **no guarda** los datos, solo valida que el archivo tenga la estructura esperada para una importación de rutinas.
        /// 
        /// **Requisitos del archivo:**
        /// - Extensión **.json**
        /// - Tamaño máximo: **10 MB**
        /// - Nombre del campo del formulario: **archivo**
        /// - Contenido: array JSON de objetos con la misma estructura que `RutinaDto`.
        ///
        /// La validación comprueba que el JSON pueda deserializarse correctamente en una lista de `RutinaDto`.
        /// No se guarda ningún dato en la base de datos, no se verifica la existencia de ejercicios, 
        /// solo se valida la estructura del JSON.
        /// 
        /// **Ejemplo de archivo válido:**
        /// ```json
        /// [
        ///   {
        ///     "nombre": "Rutina Push Pull Legs",
        ///     "descripcion": "Rutina de 3 días por semana",
        ///     "frecuenciaPorGrupo": "1-2",
        ///     "dificultad": "Intermedio",
        ///     "tiempoPorSesion": "60-75 minutos",
        ///     "imageSource": "ppl_3x4.jpg",
        ///     "semanas": [
        ///       {
        ///         "dias": [
        ///           {
        ///             "ejercicios": [
        ///               {
        ///                 "ejercicio": {
        ///                   "nombre": "Press de Banca con Barra"
        ///                 },
        ///                 "series": [
        ///                   {
        ///                     "descanso": "00:03:00",
        ///                     "repeticiones": 8,
        ///                     "porcentaje1RM": 70,
        ///                     "tipo": 5
        ///                   }
        ///                 ]
        ///               }
        ///             ]
        ///           }
        ///         ]
        ///       }
        ///     ]
        ///   }
        /// ]
        /// ```
        /// 
        /// **Respuesta exitosa (200 OK):**
        /// ```json
        /// {
        ///   "esValido": true,
        ///   "cantidadRutinas": 1,
        ///   "mensaje": "Formato válido. 1 rutina(s) detectada(s)."
        /// }
        /// ```
        /// 
        /// **Respuesta con formato inválido (200 OK):**
        /// ```json
        /// {
        ///   "esValido": false,
        ///   "cantidadRutinas": 0,
        ///   "mensaje": "Formato inválido. No se pudo deserializar el contenido en una lista de rutinas."
        /// }
        /// ```
        /// 
        /// **Respuesta con error de archivo vacío (200 OK):**
        /// ```json
        /// {
        ///   "esValido": false,
        ///   "cantidadRutinas": 0,
        ///   "mensaje": "El archivo está vacío o no se proporcionó."
        /// }
        /// ```
        /// </remarks>
        /// <param name="archivo">Archivo JSON (.json) con las rutinas a validar.</param>
        /// <returns>Resultado de la validación con detalles sobre el formato.</returns>
        /// <response code="200">Validación completada. El objeto `FormatCheckResult` indica si el formato es válido y la cantidad de rutinas.</response>
        /// <response code="400">Si no se envía archivo o el nombre no es el esperado (aunque en este endpoint se maneja como 200 con error).</response>
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
                                
                var rutinas = JsonSerializer.Deserialize<List<RutinaDto>>(jsonContent, options);
                
                if(rutinas != null)
                {
                    return Ok(new FormatCheckResult
                    {
                        EsValido = true,
                        CantidadEjercicios = 0,
                        CantidadRutinas = rutinas.Count,
                        Mensaje = $"Formato válido. {rutinas.Count} rutinas detectadas."
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
                _logger.LogWarning(ex, "Error de deserialización JSON en archivo de rutinas");
                return Ok(new FormatCheckResult
                {
                    EsValido = false,
                    CantidadRutinas = 0,
                    Mensaje = $"Formato inválido. Error en la estructura JSON: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al validar archivo de rutinas");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error inesperado al procesar la validación",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
