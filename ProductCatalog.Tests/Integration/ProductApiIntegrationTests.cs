using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Fakes;

namespace ProductCatalog.Tests.Integration;

public class ProductApiIntegrationTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IProductSource>();
                    services.RemoveAll<IAuthService>();

                    services.AddSingleton<IProductSource>(new FakeProductSource(
                        CreateProducts(),
                        CreateCategories()));

                    services.AddSingleton<IAuthService, FakeAuthService>();
                });
            });
    }

    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenTokenIsMissing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts_WhenTokenIsValid()
    {
        var client = CreateAuthorizedClient();

        var response = await client.GetAsync("/api/products");

        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductListItemDto>>();

        Assert.NotNull(products);
        Assert.Equal(3, products.Count);
    }

    [Fact]
    public async Task GetProducts_ReturnsFilteredProducts_WhenCategoryAndPriceAreProvided()
    {
        var client = CreateAuthorizedClient();

        var response = await client.GetAsync("/api/products?category=beauty&minPrice=5&maxPrice=20");

        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductListItemDto>>();

        Assert.NotNull(products);
        Assert.Single(products);
        Assert.Equal("Mascara", products[0].Name);
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "valid-test-token");

        return client;
    }

    private static IReadOnlyCollection<Product> CreateProducts()
    {
        return
        [
            new Product
            {
                Id = 1,
                Title = "Phone",
                Description = "Smartphone description",
                Price = 799,
                CategorySlug = "smartphones",
                Thumbnail = "phone.jpg",
                Images = ["phone.jpg"],
                Brand = "Test Brand",
                Rating = 4.5,
                Stock = 10
            },
            new Product
            {
                Id = 2,
                Title = "Mascara",
                Description = "Beauty product description",
                Price = 15,
                CategorySlug = "beauty",
                Thumbnail = "mascara.jpg",
                Images = ["mascara.jpg"],
                Brand = "Beauty Brand",
                Rating = 4.2,
                Stock = 50
            },
            new Product
            {
                Id = 3,
                Title = "Desk",
                Description = "Wooden desk description",
                Price = 250,
                CategorySlug = "furniture",
                Thumbnail = "desk.jpg",
                Images = ["desk.jpg"],
                Brand = "Furniture Brand",
                Rating = 4.0,
                Stock = 5
            }
        ];
    }

    private static IReadOnlyCollection<Category> CreateCategories()
    {
        return
        [
            new Category
            {
                Slug = "beauty",
                Name = "Beauty"
            },
            new Category
            {
                Slug = "smartphones",
                Name = "Smartphones"
            }
        ];
    }
}