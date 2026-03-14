using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Ilin.PartnerApp.Lib.Entities
{
    [DataContract]
    [Table("products")]
    public class Product 
    {
        [DataMember]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataMember]
        [Column("article")]
        public string Article { get; set; } = string.Empty;

        [DataMember]
        [Column("product_type")]
        public string? ProductType { get; set; }

        [DataMember]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        [Column("price")]
        public decimal Price { get; set; }

        [DataMember]
        [Column("is_active")]
        public bool IsActive { get; set; }

        public ICollection<PartnerProductSale> PartnerProductSales { get; set; } = new List<PartnerProductSale>(); 
    }
}