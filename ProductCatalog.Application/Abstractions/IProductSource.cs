using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Abstractions;

public interface IProductSource
{
    Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}