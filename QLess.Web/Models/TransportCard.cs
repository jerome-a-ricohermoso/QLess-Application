using System.ComponentModel.DataAnnotations.Schema;

namespace QLess.Web.Models
{
    public class TransportCard
    {
        public int TransportCardId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string? SeniorCitizenNumber { get; set; }
        public string? PWDIDNumber { get; set; }
        [NotMapped]
        public decimal Balance { get; set; }
    }
}
