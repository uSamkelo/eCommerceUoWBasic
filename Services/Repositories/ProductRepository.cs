using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Interfaces;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic.Services.Repositories
{
    public class ProductRepository : IProductRepository
    {
        protected DataContext _context;
        protected readonly ILogger _logger;
        protected DbSet<Product> dbSet;

        public ProductRepository(
            DataContext context,
            ILogger logger
        )
        {
            _context = context;
            _logger = logger;
            dbSet = _context.Products;
        }

        public virtual async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<Product> GetById(int id)
        {
            try
            {
                return await dbSet.FindAsync(id);
            }
            catch (Exception e)
            {

                _logger.LogError(e, "Error getting entity with id {Id}", id);
                return null;
            }
        }

        public virtual async Task<bool> AddAsync(Product entity)
        {
            try
            {
                await dbSet.AddAsync(entity);
                return true;
            }
            catch (Exception e)
            {

                _logger.LogError(e, "Error adding entity");
                return false;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await dbSet.FindAsync(id);
                if (entity != null)
                {
                    dbSet.Remove(entity);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Entity with id {Id} not found for deletion", id);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting entity with id {Id}", id);
                return false;
            }
        }

        public virtual Task<bool> UpdateAsync(Product entity)
        {
            try
            {
                dbSet.Update(entity);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating entity");
                return Task.FromResult(false);
            }
        }

    }
}
