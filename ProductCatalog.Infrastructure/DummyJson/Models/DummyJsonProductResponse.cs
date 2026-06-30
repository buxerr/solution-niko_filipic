namespace ProductCatalog.Infrastructure.DummyJson.Models;

internal class DummyJsonProductsResponse
{
    public List<DummyJsonProductDto> Products { get; set; } = [];

    public int Total { get; set; }

    public int Skip { get; set; }

    public int Limit { get; set; }
}