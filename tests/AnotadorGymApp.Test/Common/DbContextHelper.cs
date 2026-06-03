using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Test.Common
{
    public class DbContextHelper
    {
        public static async Task<AppDbContext> AppDbContextPrueba()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            var context = new AppDbContext(options);            

            return context;
        }
    }
}
