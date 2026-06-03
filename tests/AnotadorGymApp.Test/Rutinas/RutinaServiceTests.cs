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
    }
}
