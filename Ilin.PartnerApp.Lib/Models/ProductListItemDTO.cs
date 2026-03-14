namespace Ilin.PartnerApp.Lib.Models
{
    public class ProductListItemDTO
    {
        public int Id { get; set; }
        public string Article { get; set; } = string.Empty;
        public string? ProductType { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string IsActiveText => IsActive ? "Да" : "Нет";
    }
}
