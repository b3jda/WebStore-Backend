using SolrNet;
using SolrNet.Commands.Parameters;
using WebStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebStore.Services.Implementations
{
    public class SolrProductService
    {
        private readonly ISolrOperations<Product> _solr;

        public SolrProductService(ISolrOperations<Product> solr)
        {
            _solr = solr;
        }

        /// <summary>
        /// Adds or updates a product in Solr.
        /// </summary>
        public async Task AddOrUpdateProduct(Product product)
        {
            try
            {
                Console.WriteLine($"Indexing product: {product.Id}, Name: {product.Name}");

                await _solr.AddAsync(product);
                await _solr.CommitAsync();

                Console.WriteLine($"Product {product.Id} indexed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error indexing product {product.Id}: {ex.Message}");
            }
        }


        /// <summary>
        /// Deletes a product from Solr.
        /// </summary>
        public async Task DeleteProduct(int productId)
        {
            try
            {
                await _solr.DeleteAsync(productId.ToString());
                await _solr.CommitAsync();
                Console.WriteLine($"Product {productId} deleted from Solr.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product {productId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for products in Solr using filters.
        /// </summary>
        public async Task<List<Product>> SearchProducts(
            string query, int? categoryId = null, int? brandId = null,
            double? minPrice = null, double? maxPrice = null,
            string sortBy = "relevance", bool fuzzy = false)
        {
            try
            {
                string solrQueryString = fuzzy ? $"{query}~" : query;
                var solrQuery = new SolrQuery(solrQueryString);
                var queryOptions = new QueryOptions
                {
                    FilterQueries = new List<ISolrQuery>(),
                    Rows = 20,
                    OrderBy = new List<SortOrder>()
                };

                if (categoryId.HasValue)
                {
                    queryOptions.FilterQueries.Add(new SolrQueryByField("category_id", categoryId.Value.ToString()));
                }
                if (brandId.HasValue)
                {
                    queryOptions.FilterQueries.Add(new SolrQueryByField("brand_id", brandId.Value.ToString()));
                }
                if (minPrice.HasValue || maxPrice.HasValue)
                {
                    double min = minPrice ?? 0;
                    double max = maxPrice ?? double.MaxValue;
                    queryOptions.FilterQueries.Add(new SolrQueryByRange<double>("price", min, max));
                }

                switch (sortBy.ToLower())
                {
                    case "price_asc":
                        queryOptions.OrderBy.Add(new SortOrder("price", SolrNet.Order.ASC));
                        break;
                    case "price_desc":
                        queryOptions.OrderBy.Add(new SortOrder("price", SolrNet.Order.DESC));
                        break;
                    case "relevance":
                    default:
                        queryOptions.OrderBy.Add(SortOrder.Parse("score desc"));
                        break;
                }

                var results = await _solr.QueryAsync(solrQuery, queryOptions);
                Console.WriteLine($"Found {results.Count} products.");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching products in Solr: {ex.Message}");
                return new List<Product>();
            }
        }
    }
}
