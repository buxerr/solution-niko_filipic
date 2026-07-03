using ProductCatalog.Application.Abstractions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Fakes;

public class FakeProductSource : IProductSource
{
    private readonly IReadOnlyCollection<Product> _products;
    private readonly IReadOnlyCollection<Category> _categories;

    public int GetProductsCallCount { get; private set; }

    public int GetProductByIdCallCount { get; private set; }

    public int GetCategoriesCallCount { get; private set; }

    public FakeProductSource(
        IReadOnlyCollection<Product>? products = null,
        IReadOnlyCollection<Category>? categories = null)
    {
        _products = products ?? [];
        _categories = categories ?? [];
    }

    public Task<IReadOnlyCollection<Product>> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        GetProductsCallCount++;
        return Task.FromResult(_products);
    }

    public Task<Product?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        GetProductByIdCallCount++;
        var product = _products.FirstOrDefault(product => product.Id == id);

        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        GetCategoriesCallCount++;
        return Task.FromResult(_categories);
    }
}
