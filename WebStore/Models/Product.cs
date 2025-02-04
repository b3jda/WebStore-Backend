using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SolrNet.Attributes;

namespace WebStore.Models
{
    public class Product
    {
        [Key]
        [SolrUniqueKey("id")]
        public int Id { get; set; }

        [SolrField("name")]
        public string Name { get; set; }

        [SolrField("description")]
        public string Description { get; set; }

        [SolrField("price")]
        public decimal Price { get; set; }

        [SolrField("quantity")]
        public int Quantity { get; set; }

        [SolrField("original_price")]
        public decimal OriginalPrice { get; set; }

        [SolrField("is_discounted")]
        public bool IsDiscounted { get; set; }

        [SolrField("discount_percentage")]
        public double? DiscountPercentage { get; set; }

        // Foreign Key Properties (IDs)
        [SolrField("category_id")]
        public int CategoryId { get; set; }

        [SolrField("brand_id")]
        public int BrandId { get; set; }

        [SolrField("gender_id")]
        public int GenderId { get; set; }

        [SolrField("color_id")]
        public int ColorId { get; set; }

        [SolrField("size_id")]
        public int SizeId { get; set; }

        [SolrField("discount_id")]
        public int? DiscountId { get; set; }

        //  Foreign Key Attributes (Explicitly Specify the Property Name)
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        [ForeignKey(nameof(BrandId))]
        public Brand Brand { get; set; }

        [ForeignKey(nameof(GenderId))]
        public Gender Gender { get; set; }

        [ForeignKey(nameof(ColorId))]
        public Color Color { get; set; }

        [ForeignKey(nameof(SizeId))]
        public Size Size { get; set; }

        [ForeignKey(nameof(DiscountId))]
        public Discount? Discount { get; set; }
    }
}
