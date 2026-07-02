using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.DummyJson;
using ProductCatalog.Infrastructure.Options;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDummyJsonOptions(configuration);
        services.AddDummyJsonProductSource();
        services.AddDummyJsonAuthService();

        return services;
    }

    private static IServiceCollection AddDummyJsonOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DummyJsonOptions>()
            .Bind(configuration.GetSection(DummyJsonOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "DummyJson:BaseUrl must be a valid absolute URL.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddDummyJsonProductSource(
        this IServiceCollection services)
    {
        services.AddHttpClient<IProductSource, DummyJsonProductSource>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<DummyJsonOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }

    private static IServiceCollection AddDummyJsonAuthService(
        this IServiceCollection services)
    {
        services.AddHttpClient<IAuthService, DummyJsonAuthService>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<DummyJsonOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }
}