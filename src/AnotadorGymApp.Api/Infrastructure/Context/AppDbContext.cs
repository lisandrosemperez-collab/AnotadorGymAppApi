using Microsoft.EntityFrameworkCore;
using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Domain.Entities.Rutina;
using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymApp.Api.Domain.Entities.Entrenamiento;


namespace AnotadorGymAppApi.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ejercicio> Ejercicios { get; set; }
        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<RutinaSemana> RutinaSemanas { get; set; }
        public DbSet<RutinaDia> RutinaDias { get; set; }
        public DbSet<RutinaEjercicio> RutinaEjercicios { get; set; }
        public DbSet<RutinaSerie> RutinaSeries { get; set; }
        public DbSet<Musculos> Musculos { get; set; }
        public DbSet<GrupoMuscular> GrupoMusculares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.UsuarioId);
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);

                entity.Property(u => u.Email).HasMaxLength(50);

                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Rol).IsRequired().HasDefaultValue("invitado");

            });

            modelBuilder.Entity<Rutina>(r =>
            {
                r.HasKey(r => r.RutinaId);
                r.Property(r => r.RutinaId)
                     .ValueGeneratedOnAdd();   
                
                r.Property(r => r.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                r.HasIndex(r => r.Nombre).IsUnique();

                r.HasMany(r => r.Semanas)
                    .WithOne(rs => rs.Rutina)
                    .HasForeignKey(rs => rs.RutinaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });            

            modelBuilder.Entity<RutinaSemana>(rs =>
            {
                rs.HasKey(rs => rs.RutinaSemanaId);
                rs.Property(rs => rs.RutinaSemanaId)
                    .ValueGeneratedOnAdd();                    

                rs.HasMany(rs => rs.Dias)
                    .WithOne(rd => rd.RutinaSemana)
                    .HasForeignKey(rd => rd.RutinaSemanaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RutinaDia>(rd =>
            {
                rd.HasKey(rd => rd.RutinaDiaId);
                rd.Property(rd => rd.RutinaDiaId)
                     .ValueGeneratedOnAdd();                     

                rd.HasMany(rd => rd.Ejercicios)
                    .WithOne(re => re.RutinaDia)
                    .HasForeignKey(re => re.RutinaDiaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RutinaEjercicio>(re =>
            {
                re.HasKey(rs => rs.RutinaEjercicioId);
                re.Property(rs => rs.RutinaEjercicioId)
                 .ValueGeneratedOnAdd();                 

                re.HasMany(rs => rs.Series)
                .WithOne(rs => rs.RutinaEjercicio)
                .HasForeignKey(rs => rs.RutinaEjercicioId)
                .OnDelete(DeleteBehavior.Cascade);

                re.HasOne(rs => rs.Ejercicio)
                    .WithMany(rs => rs.RutinasEjercicios)
                    .HasForeignKey(rs => rs.EjercicioId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RutinaSerie>(rutinaSerie =>
            {
                rutinaSerie.HasKey(rs => rs.RutinaSerieId);
                rutinaSerie.Property(rs => rs.RutinaSerieId)
                 .ValueGeneratedOnAdd();                 

                rutinaSerie.HasOne(rs => rs.RutinaEjercicio)
                    .WithMany(rs => rs.Series)
                    .HasForeignKey(rs => rs.RutinaEjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);                
            });

            modelBuilder.Entity<Ejercicio>(ejercicio => { 
                ejercicio.HasKey(e => e.EjercicioId);
                ejercicio.Property(e => e.EjercicioId)
                 .ValueGeneratedOnAdd();             
                
                ejercicio.HasIndex(e => e.Nombre).IsUnique();

                ejercicio.Property(r => r.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                ejercicio.HasMany(e => e.RutinasEjercicios)
                    .WithOne(re => re.Ejercicio)
                    .HasForeignKey(re => re.EjercicioId)
                    .OnDelete(DeleteBehavior.Restrict);

                ejercicio.HasMany(e => e.MusculosSecundarios)
                    .WithMany(m => m.EjerciciosSecundarios)
                    .UsingEntity<Dictionary<string,object>>(
                        "EjercicioMusculoSecundarios",
                        j => j.HasOne<Musculos>()
                            .WithMany()
                            .HasForeignKey("MusculoId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Ejercicio>()
                            .WithMany()
                            .HasForeignKey("EjercicioId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => { j.HasKey("EjercicioId", "MusculoId"); }
                    );

                ejercicio.HasOne(e => e.MusculoPrimario)
                        .WithMany(m => m.EjerciciosPrimarios)
                        .HasForeignKey(e => e.MusculoPrimarioId)
                        .OnDelete(DeleteBehavior.Restrict);

                ejercicio.HasOne(e => e.GrupoMuscular)
                        .WithMany(g => g.Ejercicios)
                        .HasForeignKey(e => e.GrupoMuscularId);
            });

            modelBuilder.Entity<GrupoMuscular>(grupoMuscular => {
                grupoMuscular.HasKey(g => g.GrupoMuscularId);
                grupoMuscular.Property(g => g.GrupoMuscularId)
                 .ValueGeneratedOnAdd();                 

                grupoMuscular.HasMany(g => g.Ejercicios)
                    .WithOne(e => e.GrupoMuscular)
                    .HasForeignKey(e => e.GrupoMuscularId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Musculos>(musculo => {
                musculo.HasKey(m => m.MusculoId);
                musculo.Property(m => m.MusculoId)
                 .ValueGeneratedOnAdd();                 

                musculo.HasMany(m => m.EjerciciosPrimarios)
                        .WithOne(e => e.MusculoPrimario)
                        .HasForeignKey(e => e.MusculoPrimarioId)
                        .OnDelete(DeleteBehavior.Restrict);

                musculo.HasMany(m => m.EjerciciosSecundarios)
                .WithMany(e => e.MusculosSecundarios)
                .UsingEntity(j => j.ToTable("EjercicioMusculoSecundarios"));
                
            });

            modelBuilder.Entity<Entrenamiento>(entrenamiento =>
            {
                entrenamiento.HasKey(e => e.EntrenamientoId);

                entrenamiento.Property(e => e.EntrenamientoId)
                    .ValueGeneratedOnAdd();

                entrenamiento.Property(e => e.Fecha)
                    .IsRequired();                

                entrenamiento.Property(e => e.Notas)
                    .HasMaxLength(1000);

                entrenamiento.HasIndex(e => new
                {
                    e.UsuarioId,
                    e.Fecha
                });

                entrenamiento.HasOne(e => e.Usuario)
                    .WithMany(u => u.Entrenamientos)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);                

                entrenamiento.HasMany(e => e.Ejercicios)
                    .WithOne(ee => ee.Entrenamiento)
                    .HasForeignKey(ee => ee.EntrenamientoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EjercicioEntrenado>(ejercicioEntrenado =>
            {
                ejercicioEntrenado.HasKey(ee => ee.EjercicioEntrenadoId);
                ejercicioEntrenado.Property(ee => ee.EjercicioEntrenadoId)
                    .ValueGeneratedOnAdd();

                ejercicioEntrenado.Property(ee => ee.Orden)
                    .IsRequired();

                ejercicioEntrenado.Property(ee => ee.Notas)
                    .HasMaxLength(1000);

                ejercicioEntrenado.HasIndex(ee => ee.EntrenamientoId);
                ejercicioEntrenado.HasIndex(ee => ee.EjercicioId);

                ejercicioEntrenado.HasOne(ee => ee.Entrenamiento)
                    .WithMany(e => e.Ejercicios)
                    .HasForeignKey(ee => ee.EntrenamientoId)
                    .OnDelete(DeleteBehavior.Cascade);

                ejercicioEntrenado.HasOne(ee => ee.Ejercicio)
                    .WithMany()
                    .HasForeignKey(ee => ee.EjercicioId)
                    .OnDelete(DeleteBehavior.Restrict);

                ejercicioEntrenado.HasMany(ee => ee.Series)
                    .WithOne(s => s.EjercicioEntrenado)
                    .HasForeignKey(s => s.EjercicioEntrenadoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SerieEntrenada>(serieEntrenada =>
            {
                serieEntrenada.HasKey(s => s.SerieEntrenadaId);

                serieEntrenada.Property(s => s.SerieEntrenadaId)
                    .ValueGeneratedOnAdd();

                serieEntrenada.Property(s => s.Peso)
                    .HasPrecision(5, 2);

                serieEntrenada.HasIndex(s => s.EjercicioEntrenadoId);

                serieEntrenada.HasOne(s => s.EjercicioEntrenado)
                    .WithMany(ee => ee.Series)
                    .HasForeignKey(s => s.EjercicioEntrenadoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }   
    }
}
