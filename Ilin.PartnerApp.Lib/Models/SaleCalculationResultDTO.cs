namespace Ilin.PartnerApp.Lib.Models
{
    public class SaleCalculationResultDTO
    {
        public decimal BasePrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
