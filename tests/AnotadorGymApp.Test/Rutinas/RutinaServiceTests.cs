using AnotadorGymApp.Test.Common;
using AnotadorGymAppApi.Features.Ejercicios;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Rutinas;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymAppApi.Infrastructure.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Test.Rutinas
{
    public class RutinaServiceTests
    {
        [Fact]
        public async Task GetAllRutinas_DeberiaConsultarDb_CuandoCacheEstaVacia()
        {
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);

            context.Rutinas.Should().HaveCount(2, "Porque se agregaron 2 Rutinas Fake a la base de datos para esta prueba");

            //Mockeamos el cache para que devuelva null, simulando que no hay datos en cache
            var mockCache = new Mock<ICacheService>();
            mockCache
                .Setup(cache => cache.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var logger = NullLogger<RutinaService>.Instance;

            //Solo Se Mockean dependencias, el servicio es real

            var service = new RutinaService(
                context,                
                logger,
                mockCache.Object);

            //Act
            var rutinas = await service.GetAllRutinas();

            rutinas.Items.Should().NotBeNull();
            rutinas.DesdeCache.Should().BeFalse("Porque el cache esta vacio, entonces los datos deben venir de la base de datos");
            rutinas.Items.Should().HaveCount(2);
            rutinas.Items.Should().Contain(r => r.Nombre == "Rutina A");
            rutinas.Items.Should().Contain(x => x.Nombre == "Rutina B");

            mockCache.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Once,
                "Porque después de obtener los datos de la base de datos, el servicio debería guardar esos datos en cache");
        }
        [Fact]
        public async Task GetAllRutinas_DeberiaRetornarDatosDesdeCache()
        {

            var context = await DbContextHelper.AppDbContextPrueba();            
            var rutinas = DataHelper.ObtenerRutinasFake(context);            

            //Serializamos a Json RutinasDtos para simular lo que el cache devolvería            
            var rutinasDtoEnCache = DataHelper.CrearRutinasDtoDesdeEntities(rutinas);
            var rutinasDtoEnCacheDtoJson = System.Text.Json.JsonSerializer.Serialize(rutinasDtoEnCache);

            //Mockeamos el cache para que devuelva Rutinas
            var mockCache = new Mock<ICacheService>();
            
            mockCache
                .Setup(cache => cache.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(rutinasDtoEnCacheDtoJson);

            var logger = NullLogger<RutinaService>.Instance;

            //Solo Se Mockean dependencias, el servicio es real

            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);

            //Act
            var rutinasListResult = await service.GetAllRutinas();

            rutinasListResult.Items.Should().NotBeNull();
            rutinasListResult.DesdeCache.Should().BeTrue("Por que traemos Rutinas Desde Cache");
            rutinasListResult.Items.Should().HaveCount(2);
            rutinasListResult.Items.Should().Contain(r => r.Nombre == "Rutina A");
            rutinasListResult.Items.Should().Contain(x => x.Nombre == "Rutina B");

            mockCache.Verify(
                c => c.GetAsync(It.IsAny<string>()),
                Times.Once,
                "El service intentó leer cache ");

            mockCache.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never,
                "Porque los datos ya vienen del cache, entonces el servicio no debería intentar guardar nada nuevo en cache");
        }

        [Fact]
        public async Task GetAllRutinas_DeberiaConsultarDb_CuandoCacheDevuelvaJsonInvalidoYDesdeCacheFalse()
        {
            // Arrange
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);

            context.Rutinas.Should().HaveCount(2);

            var mockCache = new Mock<ICacheService>();
            // Cache devuelve JSON inválido
            mockCache
                .Setup(c => c.GetAsync(It.IsAny<string>()))
                .ReturnsAsync("not-a-json");

            mockCache
                .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var logger = NullLogger<RutinaService>.Instance;

            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);

            // Act
            var result = await service.GetAllRutinas();

            // Assert
            result.Items.Should().NotBeNull();
            result.DesdeCache.Should().BeFalse("Porque el cache devolvió JSON inválido y debe caer a la base de datos");
            result.Items.Should().HaveCount(2);
            result.Items.Should().Contain(r => r.Nombre == "Rutina A");
            result.Items.Should().Contain(r => r.Nombre == "Rutina B");

            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetRutina_DeberiaRetornarRutinaCuandoExiste()
        {
            // Arrange
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);

            var mockCache = new Mock<ICacheService>();            

            var logger = NullLogger<RutinaService>.Instance;

            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);

            // Act
            var result = await service.GetRutina("Rutina A");

            // Assert
            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Rutina A");
            result.RutinaId.Should().Be(1);
            result.Semanas.Should().NotBeNull();
            result.Semanas.Should().HaveCountGreaterThan(0);
            var primeraSemana = result.Semanas.First();
            primeraSemana.Dias.Should().NotBeNull();
            primeraSemana.Dias.Should().HaveCountGreaterThan(0);
            var primerDia = primeraSemana.Dias.First();
            primerDia.Ejercicios.Should().NotBeNull();
            primerDia.Ejercicios.Should().HaveCountGreaterThan(0);

            // Verificamos que el servicio no intentó leer ni escribir en cache en esta operación
            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Never);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetRutina_DeberiaRetornarNullCuandoNoExiste()
        {
            // Arrange
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);

            var mockCache = new Mock<ICacheService>();            

            var logger = NullLogger<RutinaService>.Instance;

            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);

            // Act
            var result = await service.GetRutina("Rutina inexistente");

            // Assert
            result.Should().BeNull();
            mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Never);
            mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EliminarRutinaAsync_RutinaNoExiste_DeberiaRetornarFalse()
        {
            // Arrange
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);

            var mockCache = new Mock<ICacheService>();            
            var logger = NullLogger<RutinaService>.Instance;
            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);
            // Act
            var result = await service.EliminarRutinaAsync(999); // Id que no existe
            // Assert
            result.Should().BeFalse();
            context.Rutinas.Should().HaveCount(2);
        }

        [Fact]
        public async Task EliminarRutinaAsync_RutinaExiste_DeberiaEliminarYRetornarTrue()
        {
            // Arrange
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerRutinasFake(context);
            var mockCache = new Mock<ICacheService>();
            
            var logger = NullLogger<RutinaService>.Instance;
            var service = new RutinaService(
                context,
                logger,
                mockCache.Object);
            // Pre-assert
            context.Rutinas.Should().Contain(r => r.RutinaId == 1);
            // Act
            var result = await service.EliminarRutinaAsync(1);
            // Assert
            result.Should().BeTrue();
            context.Rutinas.Should().HaveCount(1);
            context.Rutinas.Should().NotContain(r => r.RutinaId == 1);
        }
    }
}
