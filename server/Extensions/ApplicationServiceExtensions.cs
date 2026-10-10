using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Services;
using Microsoft.EntityFrameworkCore;
using server.Interfaces;
using server.Services;

namespace eCommerceUoWBasic.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<PurchaseService>();
            services.AddScoped<IProuductService, ProuductService>();

            return services;
        }
    }
}
