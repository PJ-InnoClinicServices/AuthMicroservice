using AuthMicroservice.DataAccess.IRepositories;
using AuthMicroservice.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMicroservice.BusinessLogic.Extensions;

    public static class RepositoryExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
        }
    }
