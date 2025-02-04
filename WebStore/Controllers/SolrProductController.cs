using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.Data;
using WebStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/solr")]
[ApiController]
public class SolrProductController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly SolrProductService _solrProductService;

    public SolrProductController(AppDbContext dbContext, SolrProductService solrProductService)
    {
        _dbContext = dbContext;
        _solrProductService = solrProductService;
    }

    /// <summary>
    /// Index all products from the database to Solr.
    /// </summary>
    [HttpPost("index-products")]
    public async Task<IActionResult> IndexProducts()
    {
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Gender)
            .Include(p => p.Color)
            .Include(p => p.Size)
            .ToListAsync();

        await _solrProductService.IndexProductsAsync(products);
        return Ok("Products indexed successfully in Solr.");
    }

    /// <summary>
    /// Clear all indexed products in Solr.
    /// </summary>
    [HttpDelete("clear-index")]
    public IActionResult ClearIndex()
    {
        _solrProductService.ClearSolrIndex();
        return Ok("Solr index cleared successfully.");
    }

    /// <summary>
    /// Search for products in Solr with optional min/max price filters.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string query,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter is required.");

        var results = await _solrProductService.SearchProductsAsync(query, minPrice, maxPrice, page, pageSize);
        return Ok(results);
    }
}
