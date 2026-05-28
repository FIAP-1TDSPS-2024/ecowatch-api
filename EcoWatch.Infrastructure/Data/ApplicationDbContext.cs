using EcoWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoWatch.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Ocorrencia> Ocorrencias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ocorrencia>().ToTable("OCORRENCIAS");
            modelBuilder.Entity<Ocorrencia>().HasKey(o => o.Id);
            modelBuilder.Entity<Ocorrencia>().Property(o => o.TipoOcorrencia).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Usuario>().ToTable("USUARIOS");
            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Usuario>().Property(u => u.Nome).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Usuario>().Property(u => u.Email).HasMaxLength(150).IsRequired();
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Usuario>().Property(u => u.SenhaHash).IsRequired();

            modelBuilder.Entity<Ocorrencia>()
                .HasOne(o => o.Usuario)
                .WithMany(u => u.Ocorrencias)
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
      }
    }
}