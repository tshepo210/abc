using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using abc.Models;

namespace abc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ProductEntity> Products { get; set; } = null!;
        public DbSet<CustomerEntity> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductEntity>(entity =>
            {
                entity.HasKey(e => e.RowKey);
                // Azure.Data.Tables.ETag is not a supported EF Core primitive type, ignore it for EF mapping
                entity.Ignore(e => e.ETag);
            });

            modelBuilder.Entity<CustomerEntity>(entity =>
            {
                entity.HasKey(e => e.RowKey);
                // Ignore Azure ETag property for EF mapping
                entity.Ignore(e => e.ETag);
            });
        }
    }
}
