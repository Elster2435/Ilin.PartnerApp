using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Ilin.PartnerApp.Lib.Entities
{
    [DataContract][Table("partner_types")]
    public class PartnerType 
    {
        [DataMember]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataMember]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Partner> Partners { get; set; } = new List<Partner>(); 
    }
}