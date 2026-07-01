using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Queries;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Services;

public class ProductService : IProductService
{
    private const int ShortDescriptionLength = 100;

    private readonly IProductSource _productSource;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _cache;

    public ProductService(
        IProductSource productSource,
        ILogger<ProductService> logger,
        IMemoryCache cache)
    {
        _productSource = productSource;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<ProductListItemDto>> GetProductsAsync(
        ProductQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting products with filters. Category: {Category}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}",
            query.Category,
            query.MinPrice,
            query.MaxPrice);

        ValidateQuery(query);

        var cacheKey = CreateProductsCacheKey(query);

        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<ProductListItemDto>? cachedProducts))
        {
            _logger.LogInformation("Returning products from cache. CacheKey: {CacheKey}", cacheKey);
            return cachedProducts!;
        }

        var products = await _productSource.GetProductsAsync(cancellationToken);
        var filteredProducts = products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.CategorySlug.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (query.MinPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Price <= query.MaxPrice.Value);
        }

        var result = filteredProducts
            .Select(MapToListItemDto)
            .ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Returning {ProductCount} products and storing result in cache. CacheKey: {CacheKey}",
            result.Count,
            cacheKey);

        return result;
    }

    public async Task<IReadOnlyCollection<ProductListItemDto>> SearchProductsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var normalizedSearchTerm = searchTerm.Trim().ToLowerInvariant();
        var cacheKey = $"products:search:{normalizedSearchTerm}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<ProductListItemDto>? cachedProducts))
        {
            _logger.LogInformation("Returning search results from cache. CacheKey: {CacheKey}", cacheKey);
            return cachedProducts!;
        }

        _logger.LogInformation("Searching products by term: {SearchTerm}", searchTerm);

        var products = await _productSource.GetProductsAsync(cancellationToken);

        var result = products
            .Where(product => product.Title.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase))
            .Select(MapToListItemDto)
            .ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Search term {SearchTerm} returned {ProductCount} products and was stored in cache.",
            normalizedSearchTerm,
            result.Count);

        return result;
    }

    public async Task<ProductDetailsDto?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"products:details:{id}";

        if (_cache.TryGetValue(cacheKey, out ProductDetailsDto? cachedProduct))
        {
            _logger.LogInformation("Returning product details from cache. ProductId: {ProductId}", id);
            return cachedProduct;
        }

        var product = await _productSource.GetProductByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var result = MapToDetailsDto(product);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "categories:all";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<CategoryDto>? cachedCategories))
        {
            _logger.LogInformation("Returning categories from cache.");
            return cachedCategories!;
        }

        var categories = await _productSource.GetCategoriesAsync(cancellationToken);

        var result = categories
            .Select(category => new CategoryDto
            {
                Slug = category.Slug,
                Name = category.Name
            })
            .ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    private static ProductListItemDto MapToListItemDto(Product product)
    {
        return new ProductListItemDto
        {
            Id = product.Id,
            Image = product.Thumbnail,
            Name = product.Title,
            Price = product.Price,
            ShortDescription = ShortenDescription(product.Description)
        };
    }

    private static ProductDetailsDto MapToDetailsDto(Product product)
    {
        return new ProductDetailsDto
        {
            Id = product.Id,
            Name = product.Title,
            Description = product.Description,
            Price = product.Price,
            CategorySlug = product.CategorySlug,
            Thumbnail = product.Thumbnail,
            Images = product.Images,
            Brand = product.Brand,
            Rating = product.Rating,
            Stock = product.Stock
        };
    }

    private static string ShortenDescription(string description)
    {
        if (description.Length <= ShortDescriptionLength)
        {
            return description;
        }

        return description[..ShortDescriptionLength];
    }

    private static string CreateProductsCacheKey(ProductQueryParameters query)
    {
        var category = string.IsNullOrWhiteSpace(query.Category)
            ? "all"
            : query.Category.Trim().ToLowerInvariant();

        var minPrice = query.MinPrice?.ToString() ?? "none";
        var maxPrice = query.MaxPrice?.ToString() ?? "none";

        return $"products:filter:{category}:{minPrice}:{maxPrice}";
    }

    private static void ValidateQuery(ProductQueryParameters query)
    {
        if (query.MinPrice < 0)
        {
            throw new ArgumentException("Minimum price cannot be negative.");
        }

        if (query.MaxPrice < 0)
        {
            throw new ArgumentException("Maximum price cannot be negative.");
        }

        if (query.MinPrice.HasValue &&
            query.MaxPrice.HasValue &&
            query.MinPrice > query.MaxPrice)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.");
        }
    }
}
