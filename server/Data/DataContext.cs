using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {
            
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                 .Property(product => product.Version)
                 .IsRowVersion();

            modelBuilder.Entity<IdempotencyRecord>()
                .HasIndex(record => record.Key)
                .IsUnique();
        }
    }
}
