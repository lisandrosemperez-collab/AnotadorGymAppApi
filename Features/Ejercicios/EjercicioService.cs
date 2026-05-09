using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AnotadorGymAppApi.Features.Common.Tools;
using AnotadorGymAppApi.Features.Ejercicios.Results;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public class EjercicioService : IEjercicioService
    {
        private readonly AppDbContext appDbContext;
        private readonly ICacheService cacheService;
        private const string CACHE_KEY = "Ejercicios.json";
        public EjercicioService(AppDbContext dbcontext , ICacheService cacheService)
        {
            this.cacheService = cacheService;
            appDbContext = dbcontext;
        }
        private IQueryable<Ejercicio> BaseQuery()
        {
            return appDbContext.Ejercicios.AsNoTracking();
        }
        private IQueryable<EjercicioDTO> ProjectToDto(IQueryable<Ejercicio> query)
        {
            return query.Select(e => new EjercicioDTO
            {
                Nombre = e.Nombre,
                EjercicioId = e.EjercicioId,
                Descripcion = e.Descripcion,
                UrlVideo = e.UrlVideo,
                GrupoMuscular = e.GrupoMuscular == null
                    ? null
                    : new GrupoMuscularDTO
                    {
                        Nombre = e.GrupoMuscular.Nombre
                    },
                MusculoPrimario = e.MusculoPrimario == null
                    ? null
                    : new MusculoDTO
                    {
                        Nombre = e.MusculoPrimario.Nombre
                    },
                MusculosSecundarios = e.MusculosSecundarios
                    .Select(ms => new MusculoDTO
                    {
                        Nombre = ms.Nombre
                    })
                    .ToList()
            });
        }
        
        #region Gets
        public async Task<(List<EjercicioDTO>, bool)> GetAllEjercicios()
        {
            var ejerciciosCache = await cacheService.GetAsync(CACHE_KEY);
            
            if (ejerciciosCache != null)
            {
                var data = JsonSerializer.Deserialize<List<EjercicioDTO>>(ejerciciosCache)
                   ?? new List<EjercicioDTO>();

                return (data, true);
            }

            var ejercicios = BaseQuery();
            var result = await ProjectToDto(ejercicios).ToListAsync();
            var json = System.Text.Json.JsonSerializer.Serialize(result);

            await cacheService.SetAsync(CACHE_KEY, json);

            return (result,false);
        }
        public async Task<(List<EjercicioDTO?>, int,bool)> GetEjercicios(PaginationParams pagination)
        {
            if (pagination.Page < 1) pagination.Page = 1;
            if (pagination.PageSize <= 0) pagination.PageSize = 10;
            if (pagination.PageSize > 100) pagination.PageSize = 100;

            var cache = await cacheService.GetAsync(CACHE_KEY);
            var totalCount = 0;            
            List<EjercicioDTO> todosEjercicios;

            if (!string.IsNullOrEmpty(cache))
            {
                try
                {
                    todosEjercicios = JsonSerializer.Deserialize<List<EjercicioDTO>>(cache)
                              ?? new List<EjercicioDTO>();
                }
                catch
                {
                    todosEjercicios = new List<EjercicioDTO>();
                }

                if (!string.IsNullOrWhiteSpace(pagination.Nombre))
                {
                    todosEjercicios = todosEjercicios
                        .Where(e => e.Nombre.Contains(pagination.Nombre, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                totalCount = todosEjercicios.Count;

                var items = todosEjercicios
                    .OrderBy(e => e.Nombre)
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToList();

                return (items, totalCount, true);
            }

            todosEjercicios = await ProjectToDto(BaseQuery()).ToListAsync();
            var json = JsonSerializer.Serialize(todosEjercicios);
            await cacheService.SetAsync(CACHE_KEY, json);

            var filtrados = todosEjercicios;

            if (!string.IsNullOrWhiteSpace(pagination.Nombre))
            {
                filtrados = filtrados
                    .Where(e => e.Nombre.Contains(pagination.Nombre, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            totalCount = filtrados.Count();

            var itemsDb = filtrados
                .OrderBy(e => e.Nombre)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();                                    

            return (itemsDb, totalCount,false);
        }        
        public async Task<(EjercicioDTO?, bool)> GetPorId(int id)
        {
            var ejerciciosCache = await cacheService.GetAsync(CACHE_KEY);

            if (!string.IsNullOrEmpty(ejerciciosCache))
            {                
                try
                {
                    var data = JsonSerializer.Deserialize<List<EjercicioDTO>>(ejerciciosCache)
                               ?? new List<EjercicioDTO>();

                    var ejercicio = data.FirstOrDefault(e => e.EjercicioId == id);

                    return (ejercicio, true);
                }
                catch
                {
                    // cache corrupto → ignorar
                }
            }

            var query = appDbContext.Ejercicios
                .Where(e => e.EjercicioId == id);

            var ejercicioDb = await ProjectToDto(query)
                .FirstOrDefaultAsync();

            if (ejercicioDb != null)
            {
                var todosEjercicios = await ProjectToDto(BaseQuery()).ToListAsync();

                await cacheService.SetAsync(CACHE_KEY, JsonSerializer.Serialize(todosEjercicios));
            }                
            return (ejercicioDb, false);
        }

        #endregion

        #region Sets        
        public async Task<bool> EliminarEjercicioAsync(int ejercicioId)
        {
            var ejercicio = await appDbContext.Ejercicios
                .FirstOrDefaultAsync(e => e.EjercicioId == ejercicioId);

            if (ejercicio == null) return false;

            appDbContext.Ejercicios.Remove(ejercicio);
            await appDbContext.SaveChangesAsync();

            return true;
        }
        public async Task<EjercicioSimpleDTO> ActualizarEjercicioAsync(int id, EjercicioSimpleDTO dto)
        {
            var ejercicio = await appDbContext.Ejercicios.FindAsync(id);

            if (ejercicio == null)
                return null;

            AplicarActualizacion(ejercicio, dto);

            await appDbContext.SaveChangesAsync();                

            await cacheService.DeleteAsync("Ejercicios.json");
            
            return new EjercicioSimpleDTO
            {
                EjercicioId = ejercicio.EjercicioId,
                Nombre = ejercicio.Nombre,
                Descripcion = ejercicio.Descripcion,
                UrlVideo = ejercicio.UrlVideo
            };


        }
        public async Task<ActualizarResult> ActualizarEjerciciosAsync(List<EjercicioSimpleDTO> dtos)
        {
            var result = new ActualizarResult();

            var porId = dtos.Where(x => x.EjercicioId > 0).ToList();            

            // MATCH POR ID (PRIORIDAD)

            if (porId.Any())
            {
                var ids = porId.Select(x => x.EjercicioId).ToList();

                var ejerciciosById = await appDbContext.Ejercicios
                    .Where(x => ids.Contains(x.EjercicioId))
                    .ToListAsync();

                var dict = ejerciciosById.ToDictionary(x => x.EjercicioId);

                foreach (var dto in porId)
                {
                    if (!dict.TryGetValue(dto.EjercicioId, out var ex))
                    {
                        result.NotFound++;
                        continue;
                    }                    
                    
                    AplicarActualizacion(ex, dto);
                    result.UpdatedById++;                    
                }
            }

            var porNombre = dtos.Where(x => x.EjercicioId == 0).ToList();

            // FALLBACK POR NOMBRE
            if (porNombre.Any())
            {

                var nombresNormalizados = porNombre
                    .Select(x => StringNormalize.Normalize(x.Nombre))
                    .ToList();

                var ejerciciosByName = await appDbContext.Ejercicios                
                    .ToListAsync();

                ejerciciosByName = ejerciciosByName
                    .Where(x => nombresNormalizados.Contains(StringNormalize.Normalize(x.Nombre)))
                    .ToList();

                var dictName = ejerciciosByName.ToDictionary(
                    x => StringNormalize.Normalize(x.Nombre)
                );

                foreach (var dto in porNombre)
                {
                    var dtoName = StringNormalize.Normalize(dto.Nombre);
                    
                    if (!dictName.TryGetValue(dtoName, out var ejercicio))
                    {
                        result.NotFound++;
                        continue;
                    }                    
                    AplicarActualizacion(ejercicio, dto);
                    result.UpdatedByName++;
                }
            }

            await appDbContext.SaveChangesAsync();
            
            await cacheService.DeleteAsync("Ejercicios.json");

            return result;
        }
        private void AplicarActualizacion(Ejercicio ejercicio, EjercicioSimpleDTO dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                ejercicio.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                ejercicio.Descripcion = dto.Descripcion;

            if (!string.IsNullOrWhiteSpace(dto.UrlVideo))
                ejercicio.UrlVideo = dto.UrlVideo;
        }

        #endregion
    }
}
