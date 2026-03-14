namespace Ilin.PartnerApp.Lib.Models
{
    public class PartnerSaleHistoryItemDTO
    {
        public int SaleId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateOnly SaleDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Comment { get; set; }
    }
}
