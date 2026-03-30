using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public class EjercicioService : IEjercicioService
    {
        private readonly AppDbContext appDbContext;
        public EjercicioService(AppDbContext dbcontext)
        {
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
        public async Task<List<EjercicioDTO>> GetAllEjercicios()
        {
            var ejercicios = BaseQuery();

            return await ProjectToDto(ejercicios).ToListAsync();
        }
        public async Task<(List<EjercicioDTO> items, int totalCount)> GetEjercicios(PaginationParams pagination)
        {
            if (pagination.Page < 1) pagination.Page = 1;
            if (pagination.PageSize <= 0) pagination.PageSize = 10;
            if (pagination.PageSize > 100) pagination.PageSize = 100;

            var baseQuery = BaseQuery();

            if (string.IsNullOrWhiteSpace(pagination.Nombre) == false)
            {
                baseQuery = baseQuery.Where(e => EF.Functions.Like(e.Nombre, $"%{pagination.Nombre}"));
            }

            var totalCount = await baseQuery.CountAsync();

            var query = baseQuery
            .OrderBy(e => e.Nombre)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

            var items = await ProjectToDto(query).ToListAsync();

            return (items, totalCount);
        }        
        public async Task<EjercicioDTO> GetPorId(int id)
        {
            var query = appDbContext.Ejercicios
                .Where(e => e.EjercicioId == id);

            return await ProjectToDto(query)
                .FirstOrDefaultAsync();
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
        public async Task<EjercicioDTO> ActualizarEjercicioAsync(int id, EjercicioSimpleDTO dto)
        {
            if (dto.Nombre != null && string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ApplicationException("El nombre no puede estar vacío");

            var query = appDbContext.Ejercicios
                .Where(e => e.EjercicioId == id);

            var ejercicio = await ProjectToDto(query)
                .FirstOrDefaultAsync();

            if (ejercicio == null)
                return null;

            ejercicio.Nombre = dto.Nombre ?? ejercicio.Nombre;
            ejercicio.Descripcion = dto.Descripcion ?? ejercicio.Descripcion;

            await appDbContext.SaveChangesAsync();

            return ejercicio;
        }

        #endregion
    }
}
