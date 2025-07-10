using Microsoft.EntityFrameworkCore;
using TestTaskModulBank.Models; // Убедитесь, что это правильный неймспейс для Game

namespace TestTaskModulBank.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Game> Games { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(g => g.BoardJson)
                .HasColumnType("jsonb"); 

        });

        base.OnModelCreating(modelBuilder);
    }
}