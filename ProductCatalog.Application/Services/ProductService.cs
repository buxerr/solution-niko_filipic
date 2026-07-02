using FluentValidation;
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
    private readonly IValidator<ProductQueryParameters> _validator;

    public ProductService(
        IProductSource productSource,
        ILogger<ProductService> logger,
        IMemoryCache cache,
        IValidator<ProductQueryParameters> validator)
    {
        _productSource = productSource;
        _logger = logger;
        _cache = cache;
        _validator = validator;
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

        await ValidateQuery(query, cancellationToken);

        var cacheKey = CreateProductsCacheKey(query);

        return await GetOrSetCacheAsync(cacheKey, TimeSpan.FromMinutes(5), async () =>
        {
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

            _logger.LogInformation(
                "Returning {ProductCount} products and storing result in cache. CacheKey: {CacheKey}",
                result.Count,
                cacheKey);

            return result;
        });
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

        return await GetOrSetCacheAsync(cacheKey, TimeSpan.FromMinutes(5), async () =>
        {
            _logger.LogInformation("Searching products by term: {SearchTerm}", searchTerm);

            var products = await _productSource.GetProductsAsync(cancellationToken);

            var result = products
                .Where(product => product.Title.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(MapToListItemDto)
                .ToList();

            _logger.LogInformation(
                "Search term {SearchTerm} returned {ProductCount} products and was stored in cache.",
                normalizedSearchTerm,
                result.Count);

            return result;
        });
    }

    public async Task<ProductDetailsDto?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"products:details:{id}";

        return await GetOrSetCacheAsync(cacheKey, TimeSpan.FromMinutes(10), async () =>
        {
            var product = await _productSource.GetProductByIdAsync(id, cancellationToken);

            if (product is null)
            {
                return null;
            }

            return MapToDetailsDto(product);
        });
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "categories:all";

        return await GetOrSetCacheAsync(cacheKey, TimeSpan.FromMinutes(30), async () =>
        {
            var categories = await _productSource.GetCategoriesAsync(cancellationToken);

            return categories
                .Select(category => new CategoryDto
                {
                    Slug = category.Slug,
                    Name = category.Name
                })
                .ToList();
        });
    }

    private async Task<T> GetOrSetCacheAsync<T>(
        string cacheKey,
        TimeSpan duration,
        Func<Task<T>> factory)
    {
        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            _logger.LogInformation("Cache hit. CacheKey: {CacheKey}", cacheKey);
            return cached;
        }

        var result = await factory();
        if (result is not null)
        {
            _cache.Set(cacheKey, result, duration);
        }

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

    private async Task ValidateQuery(
        ProductQueryParameters query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
