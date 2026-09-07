using eCommerceUoWBasic.Models;

namespace eCommerceUoWBasic.Interfaces
{
    public interface IUnitofWork
    {
        IProductRepository Products { get; }

        Task CompleteAsync();
    }
}
