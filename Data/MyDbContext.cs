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
    }
}