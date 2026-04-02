using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Rutinas.Results;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public class RutinaService : IRutinaService
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<RutinaService> _logger;        
        public RutinaService(AppDbContext DbContext, ILogger<RutinaService> logger)
        {
            _DbContext = DbContext;
            _logger = logger;            
        }       

        #region Gets
        public async Task<RutinaListResult> GetAllRutinas()
        {
            var count = await _DbContext.Rutinas.CountAsync();
            _logger.LogInformation("Rutinas en DB: {Count}", count);

            var rutinasQuery = _DbContext.Rutinas.AsNoTracking().OrderBy(r => r.Nombre);
            var totalCount = await rutinasQuery.CountAsync();
            
            var Rutinas = await ProjectToDto(rutinasQuery).ToListAsync();

            return new RutinaListResult{ Items = Rutinas, TotalCount = totalCount};
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
