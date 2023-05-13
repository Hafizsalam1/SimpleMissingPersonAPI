using Microsoft.EntityFrameworkCore;
using MissingPersonApp.Models;

namespace MissingPersonApp.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
            
            
        }

        public DbSet<Bio> bios { get; set; }
        public DbSet<Relative> relatives {get; set;}
        public DbSet<Kronologi> chronology {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bio>()
            .HasMany(p => p.relatives)
            .WithOne(c => c.bio)
            .HasForeignKey(c => c.id)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bio>()
            .HasMany(p => p.chronology)
            .WithOne(c => c.bio)
            .HasForeignKey(c => c.id)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}