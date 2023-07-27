using Microsoft.Extensions.DependencyInjection;

namespace OpenGraal.Net;

public static class DependencyInjection
{
    public static IServiceCollection AddGameService<TUser, TParser>(this IServiceCollection services)
        where TUser : User
        where TParser : CommandParser<TUser>
    {
        services.AddSingleton<TParser>();
        services.AddScoped<TUser>();
        services.AddHostedService<Service<TUser, TParser>>();

        return services;
    }
}