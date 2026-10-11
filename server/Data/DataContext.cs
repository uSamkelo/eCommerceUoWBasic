using eCommerceUoWBasic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace eCommerceUoWBasic.Data
{
    public class DataContext: IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {
            
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Product>()
                 .Property(product => product.Version)
                 .IsRowVersion();

            modelBuilder.Entity<IdempotencyRecord>()
                .HasIndex(record => record.Key)
                .IsUnique();
        }
    }
}
