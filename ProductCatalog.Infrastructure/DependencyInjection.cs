using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.DummyJson;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDummyJsonProductSource(configuration);

        return services;
    }

    private static IServiceCollection AddDummyJsonProductSource(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["DummyJson:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("DummyJson:BaseUrl is not configured.");
        }

        services.AddHttpClient<IProductSource, DummyJsonProductSource>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
