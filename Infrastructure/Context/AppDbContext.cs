using Microsoft.EntityFrameworkCore;
using AnotadorGymAppApi.Domain.Entities;


namespace AnotadorGymAppApi.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
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
                        .UsingEntity(j => j.ToTable("EjercicioMusculoSecundarios"));

                ejercicio.HasOne(e => e.MusculoPrimario)
                        .WithMany(m => m.EjerciciosPrimarios)
                        .HasForeignKey(e => e.MusculoPrimarioId);

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
        }   
    }
}
