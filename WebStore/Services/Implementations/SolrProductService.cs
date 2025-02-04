using SolrNet;
using SolrNet.Commands.Parameters;
using WebStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SolrProductService
{
    private readonly ISolrOperations<Product> _solr;

    public SolrProductService(ISolrOperations<Product> solr)
    {
        _solr = solr;
    }

    /// <summary>
    /// Indexes a list of products in Solr.
    /// </summary>
    public async Task IndexProductsAsync(List<Product> products)
    {
        await _solr.AddRangeAsync(products);
        await _solr.CommitAsync();
    }

    /// <summary>
    /// Deletes all products from Solr.
    /// </summary>
    public void ClearSolrIndex()
    {
        _solr.Delete("*:*");
        _solr.Commit();
    }

    /// <summary>
    /// Searches for products in Solr with optional min/max price filters.
    /// </summary>
    public async Task<List<Product>> SearchProductsAsync(string query, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 10)
    {
        var solrQuery = new SolrQuery(query);
        var filterQueries = new List<ISolrQuery>();

        // ✅ Apply Min Price Filter
        if (minPrice.HasValue)
        {
            filterQueries.Add(new SolrQuery($"price:[{minPrice.Value} TO *]"));
        }

        // ✅ Apply Max Price Filter
        if (maxPrice.HasValue)
        {
            filterQueries.Add(new SolrQuery($"price:[* TO {maxPrice.Value}]"));
        }

        var options = new QueryOptions
        {
            Start = (page - 1) * pageSize,
            Rows = pageSize,
            FilterQueries = filterQueries.Any() ? filterQueries.ToArray() : null // Apply filters only if present
        };

        var results = await _solr.QueryAsync(solrQuery, options);
        return results.ToList();
    }
}
