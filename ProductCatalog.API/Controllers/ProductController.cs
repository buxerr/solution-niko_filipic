using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Queries;
using ProductCatalog.Application.Services;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductListItemDto>>> GetProducts(
        [FromQuery] ProductQueryParameters query,
        CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.GetProductsAsync(query, cancellationToken);

            return Ok(products);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailsDto>> GetProductById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyCollection<ProductListItemDto>>> SearchProducts(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Search query is required." });
        }

        var query = new ProductQueryParameters
        {
            Search = q
        };

        var products = await _productService.GetProductsAsync(query, cancellationToken);

        return Ok(products);
    }
}