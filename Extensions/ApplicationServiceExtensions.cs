using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Services;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<PurchaseService>();

            return services;
        }
    }
}
