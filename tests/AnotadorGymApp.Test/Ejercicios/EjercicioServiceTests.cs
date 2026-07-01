using AnotadorGymApp.Test.Common;
using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymAppApi.Infrastructure.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnotadorGymApp.Test.Ejercicios
{
    public class EjercicioServiceTests
    {

        [Fact]
        public async Task GetEjercicios_FiltrandoPorNombre_DeberiaRetornarSoloCoincidenciasYTotalCorrecto()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            var pagination = new PaginationParams { Page = 1, PageSize = 10, Nombre = "Press" };

            // Act
            var (resultList, total, desdeCache) = await service.GetEjercicios(pagination);

            // Assert
            desdeCache.Should().BeFalse();
            total.Should().Be(1);
            resultList.Should().HaveCount(1);
            resultList.First().Should().NotBeNull();
            resultList.First()!.Nombre.ToLowerInvariant().Should().Contain("press");
        }

        [Fact]
        public async Task GetEjercicios_CuandoCacheContieneJsonInvalido_DeberiaRecuperarDesdeDbYMarcarDesdeCacheFalse()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync("not-a-json");
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            var pagination = new PaginationParams { Page = 1, PageSize = 10 };

            // Act
            var (resultList, total, desdeCache) = await service.GetEjercicios(pagination);

            // Assert
            desdeCache.Should().BeFalse();
            total.Should().Be(2);
            resultList.Should().HaveCount(2);
            resultList.Should().Contain(r => r.Nombre == "Press Banca");
            resultList.Should().Contain(r => r.Nombre == "Sentadilla");

            // Debe haberse intentado refrescar la caché con los datos correctos
            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetEjercicios_Paginacion_DeberiaRetornarPaginaCorrecta()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // We expect ordering by Nombre: "Press Banca" then "Sentadilla"
            var pagination = new PaginationParams { Page = 2, PageSize = 1 };

            // Act
            var (resultList, total, desdeCache) = await service.GetEjercicios(pagination);

            // Assert
            desdeCache.Should().BeFalse();
            total.Should().Be(2);
            resultList.Should().HaveCount(1);
            resultList.First().Should().NotBeNull();
            resultList.First()!.Nombre.Should().Be("Sentadilla");
        }

        [Fact]
        public async Task GetAllEjercicios_DeberiaConsultarDb_CuandoCacheEstaVacia()
        {                      
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(context);

            context.Ejercicios.Count().Should().Be(2);

            //Mockeamos el cache para que devuelva null, simulando que no hay datos en cache
            var mockCache = new Mock<ICacheService>();

            mockCache
                .Setup(cache => cache.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            //Solo Se Mockean dependencias, el servicio es real
            var service = new EjercicioService(context, mockCache.Object);

            //Act
            var (result, desdeCache) = await service.GetAllEjercicios();

            desdeCache.Should().BeFalse("Porque el cache esta vacio");
            result.Should().NotBeNull();

            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Nombre == "Press Banca");
            result.Should().Contain(x => x.Nombre == "Sentadilla");

            mockCache.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Once,
                "Porque después de obtener los datos de la base de datos, el servicio debería guardar esos datos en cache");
        }                

        [Fact]
        public async Task GetAllEjercicios_CuandoCacheContieneJsonInvalido_DeberiaRecuperarDesdeDbYMarcarDesdeCacheFalse()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync("not-a-json");
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(context);

            var service = new EjercicioService(context, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetAllEjercicios();

            // Assert
            desdeCache.Should().BeFalse("Porque si el JSON en cache es inválido, debe leerse desde la BD");
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Nombre == "Press Banca");
            result.Should().Contain(r => r.Nombre == "Sentadilla");

            // Verificamos que se intentó refrescar la cache con los datos de la BD
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetPorId_CuandoCacheContieneJsonValido_DeberiaRetornarEjercicioDesdeCacheYMarcarDesdeCacheTrue()
        {
            // Arrange
            var cacheData = new List<EjercicioDto>
            {
                new() { EjercicioId = 1, Nombre = "DesdeCache" }
            };

            var json = JsonSerializer.Serialize(cacheData);

            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(json);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            // Ensure DB has data as well but service should prefer cache
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetPorId(1);

            // Assert
            desdeCache.Should().BeTrue();
            result.Should().NotBeNull();
            result!.EjercicioId.Should().Be(1);

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPorId_CuandoCacheContieneJsonValidoYNoExisteElEjercicio_DeberiaRetornarNullYMarcarDesdeCacheTrue()
        {
            // Arrange
            var cacheData = new List<EjercicioDto>
            {
                new() { EjercicioId = 999, Nombre = "Otro" }
            };

            var json = JsonSerializer.Serialize(cacheData);

            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(json);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetPorId(1);

            // Assert
            desdeCache.Should().BeTrue();
            result.Should().BeNull();

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPorId_CuandoCacheContieneJsonInvalido_DeberiaRecuperarDesdeDbYMarcarDesdeCacheFalse()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync("not-a-json");
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetPorId(1);

            // Assert
            desdeCache.Should().BeFalse();
            result.Should().NotBeNull();
            result!.EjercicioId.Should().Be(1);

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetPorId_CuandoCacheEstaVacia_DeberiaRecuperarDesdeDbYMarcarDesdeCacheFalse()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetPorId(2);

            // Assert
            desdeCache.Should().BeFalse();
            result.Should().NotBeNull();
            result!.EjercicioId.Should().Be(2);

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetPorId_DeberiaRetornarNull_CuandoCacheEstaVaciaYNoExisteElEjercicio()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var (result, desdeCache) = await service.GetPorId(9999);

            // Assert
            desdeCache.Should().BeFalse();
            result.Should().BeNull();

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task EliminarEjercicioAsync_DeberiaRetornarFalse_CuandoNoExisteElEjercicio()
        {
            // Arrange
            var mockCache = new Mock<ICacheService>();
            var db = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(db);

            var service = new EjercicioService(db, mockCache.Object);

            // Act
            var result = await service.EliminarEjercicioAsync(9999);

            // Assert
            result.Should().BeFalse();
        }
    }
}
