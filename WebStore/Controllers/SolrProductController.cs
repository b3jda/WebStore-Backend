using Microsoft.AspNetCore.Mvc;
using WebStore.Models;
using WebStore.Services.Implementations;

namespace WebStore.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class SolrProductController : ControllerBase
    {
        private readonly SolrProductService _solrProductService;

        public SolrProductController(SolrProductService solrProductService)
        {
            _solrProductService = solrProductService;
        }

        /// <summary>
        /// Searches for products in Solr.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string query,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice,
            [FromQuery] string sortBy = "relevance",
            [FromQuery] bool fuzzy = false)
        {
            var results = await _solrProductService.SearchProducts(
                query, categoryId, brandId, minPrice, maxPrice, sortBy, fuzzy);
            return Ok(results);
        }

        /// <summary>
        /// Deletes a product from Solr.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _solrProductService.DeleteProduct(id);
            return Ok(new { message = "Product deleted from Solr" });
        }

        /// <summary>
        /// Adds or updates a product in Solr.
        /// </summary>
        [HttpPost("index")]
        public async Task<IActionResult> IndexProduct([FromBody] Product product)
        {
            await _solrProductService.AddOrUpdateProduct(product);
            return Ok(new { message = "Product indexed successfully" });
        }
    }
}
