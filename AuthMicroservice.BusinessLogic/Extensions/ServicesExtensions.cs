using AuthMicroservice.BusinessLogic.Interfaces;
using AuthMicroservice.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMicroservice.BusinessLogic.Extensions;

    public static class ServicesExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
        }
    
}