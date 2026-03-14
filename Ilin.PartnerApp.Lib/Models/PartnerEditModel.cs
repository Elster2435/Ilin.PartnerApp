namespace Ilin.PartnerApp.Lib.Models
{
    public class PartnerEditModel
    {
        public int Id { get; set; }

        public int TypeId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string? Inn { get; set; }

        public string DirectorFullname { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? LogoPath { get; set; }

        public string? SalesPlaces { get; set; }
    }
}
