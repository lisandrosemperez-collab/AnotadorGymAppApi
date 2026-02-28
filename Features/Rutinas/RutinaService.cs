using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public class RutinaService : IRutinaService
    {
        private readonly AppDbContext _DbContext;
        public RutinaService(AppDbContext DbContext)
        {
            _DbContext = DbContext;
        }

        public async Task<(List<RutinaDto> items, int totalCount)> GetAllRutinas()
        {
            var rutinasQuery = _DbContext.Rutinas.AsNoTracking().OrderBy(r => r.Nombre);
            var totalCount = await rutinasQuery.CountAsync();
            
            var Rutinas = await ProjectToDto(rutinasQuery).ToListAsync();
            return (Rutinas, totalCount);
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
                    Semanas = e.Semanas
                    .OrderBy(s => s.NumeroSemana)
                    .Select(s => new RutinaSemanaDto
                    {
                        NumeroSemana = s.NumeroSemana,
                        Dias = s.Dias
                        .OrderBy(d => d.NumeroDia)
                        .Select(d => new RutinaDiaDto
                        {
                            NumeroDia = d.NumeroDia,
                            Ejercicios = d.Ejercicios
                            .OrderBy(e => e.NumeroEjercicio)
                            .Select(ej => new RutinaEjercicioDto
                            {
                                NumeroEjercicio = ej.NumeroEjercicio,
                                Ejercicio = new EjercicioSimpleDTO
                                {
                                    Nombre = ej.Ejercicio.Nombre,
                                    Descripcion = ej.Ejercicio.Descripcion
                                },
                                Series = ej.Series
                                .OrderBy(s => s.NumeroSerie)
                                .Select(ser => new RutinaSerieDto
                                {
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
    }
}
