using Microsoft.EntityFrameworkCore;
using MinhaApi.Domain.Entities;

namespace MinhaApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);
            
            entity.HasIndex(e => e.Email)
                .IsUnique();
            
            entity.Property(e => e.Senha)
                .IsRequired();
            
            entity.Property(e => e.CriadoEm)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.AtualizadoEm)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
