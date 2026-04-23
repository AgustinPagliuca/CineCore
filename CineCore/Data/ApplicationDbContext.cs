using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CineCore.Models;

namespace CineCore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Genero> Generos { get; set; }
        public DbSet<TipoSala> TiposSala { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Butaca> Butacas { get; set; }
        public DbSet<Funcion> Funciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Pelicula>()
                .HasMany(p => p.Generos)
                .WithMany(g => g.Peliculas)
                .UsingEntity(j => j.ToTable("PeliculaGenero"));

            builder.Entity<Reserva>()
                .HasOne(r => r.Funcion)
                .WithMany(f => f.Reservas)
                .HasForeignKey(r => r.FuncionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reserva>()
                .HasOne(r => r.Butaca)
                .WithMany(b => b.Reservas)
                .HasForeignKey(r => r.ButacaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Funcion>()
                .Property(f => f.Precio)
                .HasColumnType("decimal(10,2)");

            builder.Entity<TipoSala>()
                .Property(t => t.PrecioExtra)
                .HasColumnType("decimal(10,2)");

            builder.Entity<Reserva>()
                .Property(r => r.PrecioPagado)
                .HasColumnType("decimal(10,2)");
        }
    }
}