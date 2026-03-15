namespace Ilin.PartnerApp.Lib.Models
{
    public class SaleEditModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateOnly SaleDate { get; set; }
        public decimal BasePrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Comment { get; set; }
    }
}
