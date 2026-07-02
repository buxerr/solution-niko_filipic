using ProductCatalog.Application.Abstractions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests.Fakes;

public class FakeProductSource : IProductSource
{
    private readonly IReadOnlyCollection<Product> _products;
    private readonly IReadOnlyCollection<Category> _categories;

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
        return Task.FromResult(_products);
    }

    public Task<Product?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(product => product.Id == id);

        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_categories);
    }
}
