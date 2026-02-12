using AnotadorGymAppApi.Context;
using AnotadorGymAppApi.Services.Implementations;
using AnotadorGymAppApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static System.Net.WebRequestMethods;


var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080"; 
Console.WriteLine($"🚀 Usando puerto: {port}");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);    
    serverOptions.Limits.MaxConcurrentConnections = 100;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024;
    options.AllowSynchronousIO = true;
});

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});
builder.Services.AddEndpointsApiExplorer();    

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));        
});
builder.Services.AddScoped<IEjercicioService, EjercicioService>();
builder.Services.AddScoped<IImportService, ImportService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])
            )
        };
        options.RequireHttpsMetadata = true;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "AnotadorGym API",
        Version = "v1",
        Description = "API pública para consulta de ejercicios y administración protegida.",
        Contact = new OpenApiContact
        {
            Name = "Lisandro Semperez",
            Email = "lisandrosemperez@gmail.com"
        },        
    });

    //Lectura de comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Introduce tu token JWT en el formato: **Bearer {token}**\n\n"
                    + "Ejemplo: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`\n\n"
                    + "Para obtener un token, contacta a: **lisandrosemperez@gmail.com**"
    });
    options.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer" }                
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AnotadorGym API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow
}));

app.Run();


