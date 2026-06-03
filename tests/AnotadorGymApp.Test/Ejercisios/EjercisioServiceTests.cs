using AnotadorGymApp.Test.Common;
using AnotadorGymAppApi.Domain.Entities.Ejercicio;
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

namespace AnotadorGymApp.Test.Ejericios
{
    public class EjercisioServiceTests
    {
        [Fact]
        public async Task GetEjercicios_DeberiaLimitarPageSize_A100()
        {

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
        public async Task GetAllEjercicios_DeberiaRetornarDatosDesdeCache()
        {

            var context = await DbContextHelper.AppDbContextPrueba();            

            var cacheData = new List<EjercicioDto>
            {
                new() { EjercicioId = 1, Nombre = "Press Banca" },
                new() { EjercicioId = 2, Nombre = "Sentadilla" }
            };

            var json = JsonSerializer.Serialize(cacheData);

            //Mockeamos el cache para que devuelva Datos, simulando que hay datos en Cache
            var mockCache = new Mock<ICacheService>();

            mockCache
                .Setup(c => c.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(json);

            //Solo Se Mockean dependencias, el servicio es real
            var service = new EjercicioService(context, mockCache.Object);

            //Act
            var (result, desdeCache) = await service.GetAllEjercicios();

            desdeCache.Should().BeTrue("Hay Datos en Cache");
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Nombre == "Press Banca");
            result.Should().Contain(x => x.Nombre == "Sentadilla");

            mockCache.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never,
                "No Debe Setear Cache si ya habia Datos");

            mockCache.Verify(
                c => c.GetAsync(
                    It.IsAny<string>()),
                Times.Once,
                "Debe Consultar el Cache para intentar obtener los datos antes de ir a la base de datos");
        }                
    }
}
