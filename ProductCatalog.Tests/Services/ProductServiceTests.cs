using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalog.Application.Queries;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Tests.Fakes;

namespace ProductCatalog.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_ReturnsAllProducts_WhenNoFiltersAreProvided()
    {
        var service = CreateService();

        var result = await service.GetProductsAsync(new ProductQueryParameters());

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersProductsByCategory()
    {
        var service = CreateService();

        var result = await service.GetProductsAsync(new ProductQueryParameters
        {
            Category = "beauty"
        });

        Assert.Single(result);
        Assert.Equal("Mascara", result.First().Name);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersProductsByPriceRange()
    {
        var service = CreateService();

        var result = await service.GetProductsAsync(new ProductQueryParameters
        {
            MinPrice = 100,
            MaxPrice = 900
        });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, product => product.Name == "Phone");
        Assert.Contains(result, product => product.Name == "Desk");
    }

    [Fact]
    public async Task GetProductsAsync_ThrowsArgumentException_WhenMinPriceIsGreaterThanMaxPrice()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetProductsAsync(new ProductQueryParameters
            {
                MinPrice = 1000,
                MaxPrice = 100
            }));
    }

    [Fact]
    public async Task SearchProductsAsync_ReturnsProductsMatchingTitle()
    {
        var service = CreateService();

        var result = await service.SearchProductsAsync("phone");

        Assert.Single(result);
        Assert.Equal("Phone", result.First().Name);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsShortDescriptionWithMaximum100Characters()
    {
        var longDescription = new string('a', 120);
        var service = CreateService([
            new Product
            {
                Id = 10,
                Title = "Long Description Product",
                Description = longDescription,
                Price = 10,
                CategorySlug = "test",
                Thumbnail = "image.jpg",
                Images = []
            }
        ]);

        var result = await service.GetProductsAsync(new ProductQueryParameters());

        Assert.Single(result);
        Assert.Equal(100, result.First().ShortDescription.Length);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProductDetails_WhenProductExists()
    {
        var service = CreateService();

        var result = await service.GetProductByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Phone", result!.Name);
        Assert.Equal("smartphones", result.CategorySlug);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        var service = CreateService();

        var result = await service.GetProductByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories()
    {
        var service = CreateService();

        var result = await service.GetCategoriesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, category => category.Slug == "beauty");
        Assert.Contains(result, category => category.Slug == "smartphones");
    }

    private static ProductService CreateService(
        IReadOnlyCollection<Product>? products = null,
        IReadOnlyCollection<Category>? categories = null)
    {
        var source = new FakeProductSource(
            products ?? CreateProducts(),
            categories ?? CreateCategories());

        var cache = new MemoryCache(new MemoryCacheOptions());

        return new ProductService(
            source,
            NullLogger<ProductService>.Instance,
            cache);
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
