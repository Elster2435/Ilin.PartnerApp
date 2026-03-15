using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Ilin.PartnerApp.Lib.Entities
{
    [DataContract]
    [Table("partner_product_sales")]
    public class PartnerProductSale 
    { 
        [DataMember]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataMember]
        [Column("partner_id")]
        public int PartnerId { get; set; }

        [DataMember]
        [Column("product_id")]
        public int ProductId { get; set; }

        [DataMember]
        [Column("quantity")]
        public int Quantity { get; set; }

        [DataMember]
        [Column("sale_date")]
        public DateOnly SaleDate { get; set; }

        [DataMember]
        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [DataMember]
        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [DataMember]
        [Column("comment")]
        public string? Comment { get; set; }

        [DataMember]
        [Column("base_price")]
        public decimal BasePrice { get; set; }

        [DataMember]
        [Column("discount_percent")]
        public int DiscountPercent { get; set; }

        public Partner? Partner { get; set; }

        public Product? Product { get; set; }
    }
}