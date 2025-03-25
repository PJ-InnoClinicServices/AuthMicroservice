using AuthMicroservice.DataAccess;
using AuthMicroservice.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMicroservice.BusinessLogic.Extensions;

    public static class IdentityExtensions
    {
        public static void AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<UserEntity, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 12;
                })
                .AddEntityFrameworkStores<AppDbContext>();
        }
    }
