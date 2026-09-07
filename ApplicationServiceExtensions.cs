using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Interfaces;
using eCommerceUoWBasic.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(opt =>

                opt.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                o => o.ConfigureDataSource(dataSourceBuilder =>
                {
                    dataSourceBuilder.EnableDynamicJson();
                })
            ));

            services.AddScoped<IUnitofWork, UnitofWork>();

            return services;
        }
    }
}
