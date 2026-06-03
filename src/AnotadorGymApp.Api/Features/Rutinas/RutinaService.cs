using AnotadorGymApp.Api.Features.Common.Tools;
using AnotadorGymAppApi.Domain.Entities.Rutina;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Features.Rutinas.Results;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public class RutinaService : IRutinaService
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<RutinaService> _logger;
        private readonly ICacheService _cacheService;
        private const string CACHE_KEY = "Rutinas.json";
        public RutinaService(AppDbContext DbContext, ILogger<RutinaService> logger, ICacheService cacheService)
        {
            _DbContext = DbContext;
            _logger = logger;            
            _cacheService = cacheService;
        }       

        #region Gets
        public async Task<RutinaListResult> GetAllRutinas()
        {
            var totalCount = 0;
            var rutinas = new List<RutinaDto>();
            var desdeCache = false;

            var rutinasCache = await _cacheService.GetAsync(CACHE_KEY);

            if (!string.IsNullOrWhiteSpace(rutinasCache))
            {
                rutinas = DeserealizarCache.DeserializarCache<RutinaDto>(rutinasCache);
                totalCount = rutinas.Count;
                desdeCache = true;
            }
            else
            {                
                var rutinasQuery = _DbContext.Rutinas.AsNoTracking().OrderBy(r => r.Nombre);

                totalCount = await rutinasQuery.CountAsync();                

                rutinas = await ProjectToDto(rutinasQuery).ToListAsync();

                await _cacheService.SetAsync(CACHE_KEY, System.Text.Json.JsonSerializer.Serialize(rutinas));

            }
            
            _logger.LogInformation("Rutinas: {Count}", totalCount);
            _logger.LogInformation("Rutinas en Cache: {Cache}", desdeCache);
            return new RutinaListResult { Items = rutinas, TotalCount = totalCount,DesdeCache = desdeCache };
        }
        public async Task<RutinaDto> GetRutina(string nombre)
        {
            var rutinasQuery = _DbContext.Rutinas.AsNoTracking()
                .Where(r => r.Nombre == nombre);           

            return await ProjectToDto(rutinasQuery).FirstOrDefaultAsync();
        }
        private IQueryable<RutinaDto> ProjectToDto(IQueryable<Rutina> query)
        {
            return query.Select(
                e => new RutinaDto
                {
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    TiempoPorSesion = e.TiempoPorSesion,
                    Dificultad = e.Dificultad,
                    FrecuenciaPorGrupo = e.FrecuenciaPorGrupo,
                    ImageSource = e.ImageSource,
                    RutinaId = e.RutinaId,
                    Semanas = e.Semanas
                    .OrderBy(s => s.NumeroSemana)
                    .Select(s => new RutinaSemanaDto
                    {
                        RutinaSemanaId = s.RutinaSemanaId,
                        NumeroSemana = s.NumeroSemana,
                        Dias = s.Dias
                        .OrderBy(d => d.NumeroDia)
                        .Select(d => new RutinaDiaDto
                        {
                            RutinaDiaId = d.RutinaDiaId,
                            NumeroDia = d.NumeroDia,
                            Ejercicios = d.Ejercicios
                            .OrderBy(e => e.NumeroEjercicio)
                            .Select(ej => new RutinaEjercicioDto
                            {
                                RutinaEjercicioId = ej.RutinaEjercicioId,
                                NumeroEjercicio = ej.NumeroEjercicio,
                                Ejercicio = new EjercicioSimpleDTO
                                {
                                    EjercicioId = ej.EjercicioId,
                                    Nombre = ej.Ejercicio.Nombre,
                                    Descripcion = ej.Ejercicio.Descripcion
                                },
                                Series = ej.Series
                                .OrderBy(s => s.NumeroSerie)
                                .Select(ser => new RutinaSerieDto
                                {
                                    RutinaSerieId = ser.RutinaSerieId,
                                    NumeroSerie = ser.NumeroSerie,
                                    Repeticiones = ser.Repeticiones,
                                    Porcentaje1RM = ser.Porcentaje1RM,
                                    Descanso = ser.Descanso.ToString(),
                                    Tipo = ser.Tipo
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                });
        }

        #endregion

        #region Sets        
        public async Task<bool> EliminarRutinaAsync(int rutinaId)
        {
            var rutina = await _DbContext.Rutinas
        .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);

            if (rutina == null)
                return false;

            _DbContext.Rutinas.Remove(rutina);
            await _DbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ActualizarRutinaAsync(int rutinaId, ActualizarRutinaDto dto)
        {
            var rutina = await _DbContext.Rutinas
                .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);

            if (rutina == null)
                return false;

            // Actualizar solo lo permitido
            rutina.Nombre = dto.Nombre ?? rutina.Nombre;
            rutina.Descripcion = dto.Descripcion ?? rutina.Descripcion;
            rutina.Dificultad = dto.Dificultad ?? rutina.Dificultad;
            rutina.TiempoPorSesion = dto.TiempoPorSesion ?? rutina.TiempoPorSesion;

            await _DbContext.SaveChangesAsync();

            return true;
        }

        #endregion
    }
}
