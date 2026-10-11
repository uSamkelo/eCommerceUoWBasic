using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using server.Interfaces;
using server.Models;
using server.Services;

namespace eCommerceUoWBasic.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services
                .AddIdentityCore<User>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<DataContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
                
            services.AddMemoryCache(options => options.SizeLimit = 1_000);
            services.AddSingleton<ProductCacheVersion>();
            services.AddScoped<PurchaseService>();
            services.AddScoped<IProuductService, ProuductService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}
