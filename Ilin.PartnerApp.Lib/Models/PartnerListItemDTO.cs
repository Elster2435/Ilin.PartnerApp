namespace Ilin.PartnerApp.Lib.Models
{
    public class PartnerListItemDTO
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string PartnerTypeName { get; set; } = string.Empty;

        public string DirectorFullname { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Rating { get; set; }

        public int TotalQuantity { get; set; }

        public int DiscountPercent { get; set; }

        public string DiscountText => $"{DiscountPercent}%";
    }
}
