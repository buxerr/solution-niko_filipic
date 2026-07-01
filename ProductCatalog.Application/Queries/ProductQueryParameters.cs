namespace ProductCatalog.Application.Queries;

public class ProductQueryParameters
{
    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

}