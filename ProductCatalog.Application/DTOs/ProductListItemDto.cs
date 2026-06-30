namespace ProductCatalog.Application.DTOs;

public class ProductListItemDto
{
    public int Id { get; set; }

    public string Image { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string ShortDescription { get; set; } = string.Empty;
}
