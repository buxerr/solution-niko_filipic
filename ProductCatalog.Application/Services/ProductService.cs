using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Queries;
using ProductCatalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ProductCatalog.Application.Services;

public class ProductService : IProductService
{
    private const int ShortDescriptionLength = 100;

    private readonly IProductSource _productSource;

    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductSource productSource,
        ILogger<ProductService> logger)
    {
        _productSource = productSource;
        _logger = logger;
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

        _logger.LogInformation("Returning {ProductCount} products.", result.Count);

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

        _logger.LogInformation("Searching products by term: {SearchTerm}", searchTerm);

        var products = await _productSource.GetProductsAsync(cancellationToken);

        var result = products
            .Where(product => product.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Select(MapToListItemDto)
            .ToList();

        _logger.LogInformation(
            "Search term {SearchTerm} returned {ProductCount} products.",
            searchTerm,
            result.Count);

        return result;
    }

    public async Task<ProductDetailsDto?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productSource.GetProductByIdAsync(id, cancellationToken);

        return product is null
            ? null
            : MapToDetailsDto(product);
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _productSource.GetCategoriesAsync(cancellationToken);

        return categories
            .Select(category => new CategoryDto
            {
                Slug = category.Slug,
                Name = category.Name
            })
            .ToList();
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