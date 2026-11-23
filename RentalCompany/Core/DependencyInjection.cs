using Core.Authentication;
using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddData(configuration);
        services.AddOptions<SessionOptions>().Bind(configuration.GetSection("Authentication:Session"))
            .Validate(options => options.LifetimeMinutes is >= 1 and <= 1440, "Czas sesji musi wynosić od 1 do 1440 minut.")
            .ValidateOnStart();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserAccess, UserAccess>();
        return services;
    }
}
