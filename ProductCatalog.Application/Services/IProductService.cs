using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Queries;

namespace ProductCatalog.Application.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductListItemDto>> GetProductsAsync(
        ProductQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailsDto?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}