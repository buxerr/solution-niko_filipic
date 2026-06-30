namespace ProductCatalog.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string CategorySlug { get; set; } = string.Empty;

    public string Thumbnail { get; set; } = string.Empty;

    public List<string> Images { get; set; } = [];

    public string? Brand { get; set; }

    public double? Rating { get; set; }

    public int? Stock { get; set; }
}