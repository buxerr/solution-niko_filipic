using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.DummyJson.Models;

namespace ProductCatalog.Infrastructure.DummyJson;

public class DummyJsonProductSource : IProductSource
{
    private const int PageSize = 100;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    private readonly ILogger<DummyJsonProductSource> _logger;

    public DummyJsonProductSource(
        HttpClient httpClient,
        ILogger<DummyJsonProductSource> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Product>> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching products from DummyJSON.");

        var products = new List<Product>();
        var skip = 0;

        while (true)
        {
            var response = await _httpClient.GetFromJsonAsync<DummyJsonProductsResponse>(
                $"products?limit={PageSize}&skip={skip}",
                JsonOptions,
                cancellationToken);

            if (response is null)
            {
                throw new InvalidOperationException("DummyJSON products response was empty.");
            }

            products.AddRange(response.Products.Select(MapToProduct));

            if (products.Count >= response.Total || response.Products.Count == 0)
            {
                break;
            }

            skip += PageSize;
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching product {ProductId} from DummyJSON.", id);

        try
        {
            var product = await _httpClient.GetFromJsonAsync<DummyJsonProductDto>(
                $"products/{id}",
                JsonOptions,
                cancellationToken);

            return product is null ? null : MapToProduct(product);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching categories from DummyJSON.");

        var categories = await _httpClient.GetFromJsonAsync<List<DummyJsonCategoryDto>>(
            "products/categories",
            JsonOptions,
            cancellationToken);

        return categories is null
            ? []
            : categories.Select(MapToCategory).ToList();
    }

    private static Product MapToProduct(DummyJsonProductDto product)
    {
        return new Product
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            CategorySlug = product.Category,
            Thumbnail = product.Thumbnail,
            Images = product.Images,
            Brand = product.Brand,
            Rating = product.Rating,
            Stock = product.Stock
        };
    }

    private static Category MapToCategory(DummyJsonCategoryDto category)
    {
        return new Category
        {
            Slug = category.Slug,
            Name = category.Name
        };
    }
}