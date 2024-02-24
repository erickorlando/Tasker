using Microsoft.EntityFrameworkCore;
using Tasker.Entities;

namespace Tasker.DataAccess
{
    public class TaskerDbContext : DbContext
    {
        public TaskerDbContext(DbContextOptions<TaskerDbContext> options)
            :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Factura>()
                .ToTable("Factura");

            modelBuilder.Entity<Numerador>()
                .ToTable("Numerador");

            // Data seeding
            modelBuilder.Entity<Numerador>()
                .HasData(new List<Numerador>()
                {
                    new() { Id = 1, Tabla = "Factura", UltimoNumero = 5913 }
                });
        }
    }
}
