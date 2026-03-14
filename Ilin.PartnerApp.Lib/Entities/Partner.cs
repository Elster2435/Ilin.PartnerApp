using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Ilin.PartnerApp.Lib.Entities
{
    [DataContract]
    [Table("partners")]
    public class Partner
    {
        [DataMember]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataMember]
        [Column("type_id")]
        public int TypeId { get; set; }

        [DataMember]
        [Column("company_name")]
        public string CompanyName { get; set; } = string.Empty;

        [DataMember]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [DataMember]
        [Column("inn")]
        public string? Inn { get; set; }

        [DataMember]
        [Column("director_fullname")]
        public string DirectorFullname { get; set; } = string.Empty;

        [DataMember]
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [DataMember]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [DataMember]
        [Column("rating")]
        public int Rating { get; set; }

        [DataMember]
        [Column("logo_path")]
        public string? LogoPath { get; set; }

        [DataMember]
        [Column("sales_places")]
        public string? SalesPlaces { get; set; }

        [DataMember]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public PartnerType? PartnerType { get; set; }

        public ICollection<PartnerProductSale> PartnerProductSales { get; set; } = new List<PartnerProductSale>();
    }
}