namespace Ilin.PartnerApp.Lib.Models
{
    public class SaleEditModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateOnly SaleDate { get; set; }
        public string? Comment { get; set; }
    }
}
