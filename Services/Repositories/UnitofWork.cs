using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Interfaces;

namespace eCommerceUoWBasic.Services.Repositories
{
    public class UnitofWork: IUnitofWork, IDisposable
    {
        private readonly DataContext _context;
        public readonly ILogger _logger;

        public IProductRepository Products {  get; private set; }

        public UnitofWork(DataContext context,ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("logs");

            Products = new ProductRepository(_context, _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
