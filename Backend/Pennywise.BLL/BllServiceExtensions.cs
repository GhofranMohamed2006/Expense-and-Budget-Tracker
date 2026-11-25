using Microsoft.Extensions.DependencyInjection;
using Pennywise.BLL.Services;

namespace Pennywise.BLL;

public static class BllServiceExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, JwtSettings jwt)
    {
        services.AddSingleton(jwt);
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProfileService, ProfileService>();
        return services;
    }
}
