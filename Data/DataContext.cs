using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {
            
        }

        public DbSet<Product> Products { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Product>()
        //         .HasKey(p => p.Id);
        //}
    }
}
